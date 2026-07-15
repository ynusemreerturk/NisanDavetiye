using System.IO.Compression;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.BLL.Security;
using NisanDavetiye.DAL.Entities;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public class GaleriService : IGaleriService
{
    private const int MaxFilesPerRequest = 10;
    private const long MaxFileBytes = 15 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif",
    };

    private readonly IDavetiyeRepository _repo;
    private readonly GaleriStorageOptions _storage;
    private readonly IMediaUrlSigner _mediaSigner;

    public GaleriService(
        IDavetiyeRepository repo,
        IOptions<GaleriStorageOptions> storageOptions,
        IMediaUrlSigner mediaSigner)
    {
        _repo = repo;
        _storage = storageOptions.Value;
        _mediaSigner = mediaSigner;
    }

    public async Task<GaleriUploadResultDto> UploadAsync(
        IReadOnlyList<GaleriUploadFile> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
            throw new ArgumentException("En az bir fotoğraf seçin.");

        if (files.Count > MaxFilesPerRequest)
            throw new ArgumentException($"Tek seferde en fazla {MaxFilesPerRequest} fotoğraf yükleyebilirsiniz.");

        if (string.IsNullOrWhiteSpace(_storage.AbsoluteUploadDirectory))
            throw new InvalidOperationException("Galeri depolama klasörü yapılandırılmamış.");

        var today = DateTime.UtcNow.Date;
        var prefix = _storage.PublicUrlPrefix.TrimEnd('/');
        var uploadsToday = await _repo.CountGuestUploadsSinceAsync(today, prefix);
        if (uploadsToday + files.Count > _storage.MaxDailyUploadCount)
            throw new ArgumentException("Günlük fotoğraf yükleme limitine ulaşıldı. Lütfen daha sonra tekrar deneyin.");

        Directory.CreateDirectory(_storage.AbsoluteUploadDirectory);

        var nextSira = await _repo.GetNextGaleriSiraAsync();
        var uploadedNames = new List<string>();
        var newItems = new List<GaleriResmi>();
        var now = DateTime.UtcNow;

        // Tek geçiş: doğrula → doğrudan diske stream (MemoryStream ile çift kopya yok).
        foreach (var file in files)
        {
            if (file.Length <= 0)
                throw new ArgumentException($"{file.FileName} boş dosya.");

            if (file.Length > MaxFileBytes)
                throw new ArgumentException($"{file.FileName} dosyası çok büyük (en fazla 15 MB).");

            if (!AllowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException($"{file.FileName} desteklenmeyen bir dosya türü.");

            var (ok, detectedType) = await ImageFileValidator.TryDetectAsync(
                file.ContentType,
                file.Content,
                cancellationToken);

            if (!ok)
                throw new ArgumentException($"{file.FileName} geçerli bir görüntü dosyası değil.");

            if (!file.Content.CanSeek)
                throw new ArgumentException($"{file.FileName} okunamadı. Lütfen tekrar deneyin.");

            var ext = ExtensionFromContentType(detectedType);
            var id = Guid.NewGuid().ToString("N")[..8];
            var storedName = $"{now:yyyyMMdd-HHmmss}-{id}{ext}";
            var diskPath = Path.Combine(_storage.AbsoluteUploadDirectory, storedName);

            file.Content.Position = 0;
            await using (var fs = new FileStream(
                             diskPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 81920,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await file.Content.CopyToAsync(fs, cancellationToken);
            }

            var displayName = SanitizeDisplayName(file.FileName);
            var url = $"{prefix}/{storedName}";

            newItems.Add(new GaleriResmi
            {
                Url = url,
                AltMetin = displayName,
                Sira = nextSira++,
                Onaylandi = false,
                YuklemeTarihi = now,
            });

            uploadedNames.Add(displayName);
        }

        await _repo.AddGaleriResimleriAsync(newItems);

        return new GaleriUploadResultDto(
            uploadedNames.Count,
            uploadedNames,
            "Fotoğraflar yüklendi. Yönetici onayından sonra yayınlanacaktır.");
    }

    public async Task<GaleriDto> ApproveUploadedPhotoAsync(int id)
    {
        var item = await _repo.GetGaleriResmiByIdAsync(id)
            ?? throw new KeyNotFoundException("Fotoğraf bulunamadı.");

        if (!_mediaSigner.IsGuestUploadUrl(item.Url))
            throw new InvalidOperationException("Bu kayıt misafir yüklemesi değil.");

        await _repo.SetGaleriOnayAsync(id, true);
        item.Onaylandi = true;
        return MapDto(item, forAdmin: true);
    }

    public async Task<GaleriSilResultDto> RejectUploadedPhotoAsync(int id)
    {
        var item = await _repo.GetGaleriResmiByIdAsync(id)
            ?? throw new KeyNotFoundException("Fotoğraf bulunamadı.");

        if (!_mediaSigner.IsGuestUploadUrl(item.Url))
            throw new InvalidOperationException("Bu kayıt misafir yüklemesi değil.");

        DeleteDiskFile(item.Url);
        await _repo.DeleteGaleriResmiAsync(id);
        return new GaleriSilResultDto(1);
    }

    public async Task<(byte[] Content, string FileName)> ExportUploadedZipAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await GetUploadedItemsAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("İndirilecek fotoğraf yok.");

        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                var diskPath = ResolveDiskPath(item.Url);
                if (diskPath is null || !File.Exists(diskPath))
                    continue;

                var entryName = UniqueZipEntryName(item, Path.GetFileName(diskPath), usedNames);
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await using var fileStream = File.OpenRead(diskPath);
                await fileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        if (memory.Length == 0)
            throw new InvalidOperationException("Fotoğraf dosyaları diskte bulunamadı.");

        var fileName = $"nisandavetiye-galeri-{DateTime.Now:yyyyMMdd-HHmm}.zip";
        return (memory.ToArray(), fileName);
    }

    public async Task<GaleriSilResultDto> DeleteUploadedPhotoAsync(int id)
    {
        var item = await _repo.GetGaleriResmiByIdAsync(id)
            ?? throw new KeyNotFoundException("Fotoğraf bulunamadı.");

        if (!_mediaSigner.IsGuestUploadUrl(item.Url))
            throw new InvalidOperationException("Bu kayıt misafir yüklemesi değil.");

        DeleteDiskFile(item.Url);
        await _repo.DeleteGaleriResmiAsync(id);
        return new GaleriSilResultDto(1);
    }

    public async Task<GaleriSilResultDto> DeleteAllUploadedPhotosAsync()
    {
        var items = await GetUploadedItemsAsync();
        var deleted = 0;

        foreach (var item in items)
        {
            if (DeleteDiskFile(item.Url))
                deleted++;

            await _repo.DeleteGaleriResmiAsync(item.Id);
        }

        return new GaleriSilResultDto(deleted);
    }

    private GaleriDto MapDto(GaleriResmi item, bool forAdmin)
    {
        var misafir = _mediaSigner.IsGuestUploadUrl(item.Url);
        var url = misafir
            ? _mediaSigner.SignGuestFile(_mediaSigner.TryGetFileName(item.Url)!, forAdmin)
            : item.Url;

        return new GaleriDto(item.Id, url, item.AltMetin, item.Sira, item.Onaylandi, misafir);
    }

    private async Task<IReadOnlyList<GaleriResmi>> GetUploadedItemsAsync()
    {
        var all = await _repo.GetGaleriAsync();
        return all.Where(g => _mediaSigner.IsGuestUploadUrl(g.Url)).ToList();
    }

    private string? ResolveDiskPath(string url)
    {
        var fileName = _mediaSigner.TryGetFileName(url);
        if (fileName is null)
            return null;

        return Path.Combine(_storage.AbsoluteUploadDirectory, fileName);
    }

    private bool DeleteDiskFile(string url)
    {
        var diskPath = ResolveDiskPath(url);
        if (diskPath is null || !File.Exists(diskPath))
            return false;

        File.Delete(diskPath);
        return true;
    }

    private static string UniqueZipEntryName(GaleriResmi item, string storedName, HashSet<string> usedNames)
    {
        var baseName = string.IsNullOrWhiteSpace(item.AltMetin)
            ? storedName
            : Path.GetFileName(item.AltMetin);

        if (string.IsNullOrWhiteSpace(baseName))
            baseName = storedName;

        var ext = Path.GetExtension(storedName);
        if (string.IsNullOrWhiteSpace(Path.GetExtension(baseName)) && !string.IsNullOrWhiteSpace(ext))
            baseName += ext;

        var candidate = baseName;
        var counter = 1;
        while (!usedNames.Add(candidate))
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(baseName);
            var extension = Path.GetExtension(baseName);
            candidate = $"{nameWithoutExt}-{counter}{extension}";
            counter++;
        }

        return candidate;
    }

    private static string ExtensionFromContentType(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            _ => ".jpg",
        };

    private static string SanitizeDisplayName(string fileName)
    {
        var name = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(name))
            return "Fotoğraf";

        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name;
    }
}

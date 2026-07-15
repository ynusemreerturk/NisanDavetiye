using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public interface IDriveOffloadService
{
    Task<GaleriDriveStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Yerel diskteki tüm bekleyen misafir fotoğraflarını kuyruğa alır (manuel tetikleme).</summary>
    Task<GaleriDriveOffloadResultDto> EnqueuePendingAsync(CancellationToken cancellationToken = default);

    /// <summary>Yerel klasör boyutu eşiği aşıyorsa bekleyenleri kuyruğa alır (otomatik).</summary>
    Task<int> EnqueuePendingIfOverThresholdAsync(CancellationToken cancellationToken = default);
}

public class DriveOffloadService : IDriveOffloadService
{
    private readonly IDavetiyeRepository _repo;
    private readonly IDriveOffloadQueue _queue;
    private readonly IDriveStorageService _drive;
    private readonly GaleriStorageOptions _storage;
    private readonly DriveOffloadOptions _options;

    public DriveOffloadService(
        IDavetiyeRepository repo,
        IDriveOffloadQueue queue,
        IDriveStorageService drive,
        IOptions<GaleriStorageOptions> storageOptions,
        IOptions<DriveOffloadOptions> offloadOptions)
    {
        _repo = repo;
        _queue = queue;
        _drive = drive;
        _storage = storageOptions.Value;
        _options = offloadOptions.Value;
    }

    public async Task<GaleriDriveStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var localBytes = ComputeLocalUsedBytes();
        var prefix = _storage.PublicUrlPrefix.TrimEnd('/');

        var all = await _repo.GetGaleriAsync();
        var guest = all.Where(g => g.Url.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase)).ToList();
        var offloaded = guest.Count(g => !string.IsNullOrEmpty(g.DriveFileId));
        var pending = guest.Count - offloaded;

        return new GaleriDriveStatusDto(
            DriveEnabled: _drive.IsConfigured,
            LocalUsedBytes: localBytes,
            ThresholdBytes: _options.ThresholdBytes,
            ThresholdMegabytes: _options.ThresholdMegabytes,
            PendingCount: pending,
            OffloadedCount: offloaded,
            OverThreshold: localBytes >= _options.ThresholdBytes);
    }

    public async Task<GaleriDriveOffloadResultDto> EnqueuePendingAsync(CancellationToken cancellationToken = default)
    {
        if (!_drive.IsConfigured)
            throw new InvalidOperationException("Google Drive entegrasyonu yapılandırılmamış veya kapalı.");

        var queued = await EnqueueAllPendingAsync(cancellationToken);
        return new GaleriDriveOffloadResultDto(
            queued,
            queued == 0
                ? "Aktarılacak yeni fotoğraf bulunamadı."
                : $"{queued} fotoğraf Drive aktarım kuyruğuna alındı. Aktarım arka planda yapılacaktır.");
    }

    public async Task<int> EnqueuePendingIfOverThresholdAsync(CancellationToken cancellationToken = default)
    {
        if (!_drive.IsConfigured)
            return 0;

        var localBytes = ComputeLocalUsedBytes();
        if (localBytes < _options.ThresholdBytes)
            return 0;

        return await EnqueueAllPendingAsync(cancellationToken);
    }

    private async Task<int> EnqueueAllPendingAsync(CancellationToken cancellationToken)
    {
        var prefix = _storage.PublicUrlPrefix.TrimEnd('/');
        var pending = await _repo.GetGuestUploadsPendingDriveAsync(prefix);
        if (pending.Count == 0)
            return 0;

        var ids = pending.Select(p => p.Id).ToList();
        await _queue.EnqueueBatchAsync(ids, cancellationToken);
        return ids.Count;
    }

    private long ComputeLocalUsedBytes()
    {
        var dir = _storage.AbsoluteUploadDirectory;
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return 0;

        long total = 0;
        foreach (var file in Directory.EnumerateFiles(dir))
        {
            try
            {
                total += new FileInfo(file).Length;
            }
            catch
            {
                // dosya silinmiş olabilir; yok say
            }
        }

        return total;
    }
}

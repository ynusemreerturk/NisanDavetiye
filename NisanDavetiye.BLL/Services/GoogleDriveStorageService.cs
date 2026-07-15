using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;

namespace NisanDavetiye.BLL.Services;

/// <summary>
/// OAuth 2.0 refresh token ile kişisel Google Drive'a yükleme yapar.
/// Servis singleton'dır; DriveService/kimlik bilgisi tembel oluşturulur ve
/// access token'ı otomatik yeniler.
/// </summary>
public class GoogleDriveStorageService : IDriveStorageService, IDisposable
{
    private readonly DriveOffloadOptions _options;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private DriveService? _service;

    public GoogleDriveStorageService(IOptions<DriveOffloadOptions> options) => _options = options.Value;

    public bool IsConfigured => _options.IsConfigured;

    public string BuildViewUrl(string fileId) =>
        $"https://drive.google.com/uc?export=view&id={fileId}";

    public async Task<string> UploadAsync(
        string localPath,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var service = await GetServiceAsync(cancellationToken);

        var metadata = new Google.Apis.Drive.v3.Data.File { Name = fileName };
        if (!string.IsNullOrWhiteSpace(_options.FolderId))
            metadata.Parents = new[] { _options.FolderId };

        await using var stream = System.IO.File.OpenRead(localPath);
        var request = service.Files.Create(metadata, stream, contentType);
        request.Fields = "id";

        var progress = await request.UploadAsync(cancellationToken);
        if (progress.Status != Google.Apis.Upload.UploadStatus.Completed)
            throw progress.Exception ?? new InvalidOperationException("Google Drive yüklemesi tamamlanamadı.");

        var fileId = request.ResponseBody?.Id
            ?? throw new InvalidOperationException("Google Drive dosya kimliği alınamadı.");

        if (_options.MakePublic)
        {
            var permission = new Permission { Type = "anyone", Role = "reader" };
            await service.Permissions.Create(permission, fileId).ExecuteAsync(cancellationToken);
        }

        return fileId;
    }

    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            return;

        var service = await GetServiceAsync(cancellationToken);
        await service.Files.Delete(fileId).ExecuteAsync(cancellationToken);
    }

    private async Task<DriveService> GetServiceAsync(CancellationToken cancellationToken)
    {
        if (_service is not null)
            return _service;

        if (!_options.IsConfigured)
            throw new InvalidOperationException("Google Drive yapılandırması eksik (ClientId/ClientSecret/RefreshToken).");

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_service is not null)
                return _service;

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret,
                },
                Scopes = new[] { DriveService.Scope.DriveFile },
            });

            var token = new TokenResponse { RefreshToken = _options.RefreshToken };
            var credential = new UserCredential(flow, "user", token);

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "NisanDavetiye",
            });

            return _service;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public void Dispose()
    {
        _service?.Dispose();
        _initLock.Dispose();
        GC.SuppressFinalize(this);
    }
}

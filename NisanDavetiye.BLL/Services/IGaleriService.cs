using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.BLL.Services;

public interface IGaleriService
{
    Task<GaleriUploadResultDto> UploadAsync(
        IReadOnlyList<GaleriUploadFile> files,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> ExportUploadedZipAsync(
        CancellationToken cancellationToken = default);

    Task<GaleriSilResultDto> DeleteUploadedPhotoAsync(int id);

    Task<GaleriSilResultDto> DeleteAllUploadedPhotosAsync();

    Task<GaleriDto> ApproveUploadedPhotoAsync(int id);

    Task<GaleriSilResultDto> RejectUploadedPhotoAsync(int id);
}

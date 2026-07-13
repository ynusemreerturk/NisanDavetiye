namespace NisanDavetiye.BLL.Services;

public interface IMediaUrlSigner
{
    bool IsGuestUploadUrl(string url);
    string? TryGetFileName(string url);
    string SignGuestFile(string fileName, bool forAdmin);
    bool TryValidate(string fileName, long expUnix, string signature);
}

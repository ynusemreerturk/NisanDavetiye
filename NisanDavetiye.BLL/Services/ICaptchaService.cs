namespace NisanDavetiye.BLL.Services;

public interface ICaptchaService
{
    Task ValidateOrThrowAsync(string? token, string? remoteIp, CancellationToken cancellationToken = default);
}

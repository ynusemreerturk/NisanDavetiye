namespace NisanDavetiye.BLL.Services;

public interface IPanelAuthService
{
    Task<bool> IsValidPanelUidAsync(string? provided);
}

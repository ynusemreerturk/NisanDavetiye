using NisanDavetiye.BLL.Security;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public class PanelAuthService : IPanelAuthService
{
    private readonly IDavetiyeRepository _repo;

    public PanelAuthService(IDavetiyeRepository repo) => _repo = repo;

    public async Task<bool> IsValidPanelUidAsync(string? provided)
    {
        if (string.IsNullOrWhiteSpace(provided) || provided.Length != 32)
            return false;

        var stored = await _repo.GetPanelUidAsync();
        return SecureCompare.Equals(stored, provided.Trim());
    }
}

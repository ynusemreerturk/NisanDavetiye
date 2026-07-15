using NisanDavetiye.BLL.Security;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public class InviteAuthService : IInviteAuthService
{
    private readonly IDavetiyeRepository _repo;

    public InviteAuthService(IDavetiyeRepository repo) => _repo = repo;

    public async Task<bool> IsValidDavetKeyAsync(string? provided)
    {
        if (!InviteSlug.IsValid(provided))
            return false;

        var stored = await _repo.GetDavetUidAsync();
        return SecureCompare.Equals(stored, provided!.Trim());
    }
}

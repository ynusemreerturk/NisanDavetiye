using NisanDavetiye.DAL.Entities;

namespace NisanDavetiye.DAL.Repositories;

public interface IRsvpRepository
{
    Task<RsvpKayit> AddAsync(RsvpKayit kayit);
    Task<IReadOnlyList<RsvpKayit>> GetAllAsync();
    Task<IReadOnlyList<RsvpKayit>> GetVisibleForAdminAsync();
    Task<RsvpKayit?> GetByIdAsync(int id);
    Task<bool> ExistsByTelefonAsync(string telefon);
    Task HideFromAdminAsync(int id);
}

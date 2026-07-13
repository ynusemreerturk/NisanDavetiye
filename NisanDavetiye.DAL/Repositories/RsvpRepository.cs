using Microsoft.EntityFrameworkCore;
using NisanDavetiye.DAL.Data;
using NisanDavetiye.DAL.Entities;

namespace NisanDavetiye.DAL.Repositories;

public class RsvpRepository : IRsvpRepository
{
    private readonly NisanDavetiyeDbContext _db;

    public RsvpRepository(NisanDavetiyeDbContext db) => _db = db;

    public async Task<RsvpKayit> AddAsync(RsvpKayit kayit)
    {
        _db.RsvpKayitlari.Add(kayit);
        await _db.SaveChangesAsync();
        return kayit;
    }

    public async Task<IReadOnlyList<RsvpKayit>> GetAllAsync() =>
        await _db.RsvpKayitlari.OrderByDescending(r => r.OlusturmaTarihi).ToListAsync();

    public async Task<IReadOnlyList<RsvpKayit>> GetVisibleForAdminAsync() =>
        await _db.RsvpKayitlari
            .Where(r => !r.AdminListedenGizli)
            .OrderByDescending(r => r.OlusturmaTarihi)
            .ToListAsync();

    public Task<RsvpKayit?> GetByIdAsync(int id) =>
        _db.RsvpKayitlari.FirstOrDefaultAsync(r => r.Id == id);

    public Task<bool> ExistsByTelefonAsync(string telefon) =>
        _db.RsvpKayitlari.AnyAsync(r => r.Telefon == telefon);

    public async Task HideFromAdminAsync(int id)
    {
        var kayit = await _db.RsvpKayitlari.FindAsync(id);
        if (kayit is null) return;

        kayit.AdminListedenGizli = true;
        await _db.SaveChangesAsync();
    }
}

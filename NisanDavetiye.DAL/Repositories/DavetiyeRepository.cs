using Microsoft.EntityFrameworkCore;
using NisanDavetiye.DAL.Data;
using NisanDavetiye.DAL.Entities;

namespace NisanDavetiye.DAL.Repositories;

public class DavetiyeRepository : IDavetiyeRepository
{
    private readonly NisanDavetiyeDbContext _db;

    public DavetiyeRepository(NisanDavetiyeDbContext db) => _db = db;

    public Task<DavetiyeAyarlari?> GetAyarlariAsync() =>
        _db.DavetiyeAyarlari.FirstOrDefaultAsync();

    public Task<DavetiyeAyarlari?> GetAyarlariByUidAsync(string uid) =>
        _db.DavetiyeAyarlari.FirstOrDefaultAsync(a => a.DavetUid == uid);

    public async Task<string?> GetDavetUidAsync()
    {
        var ayar = await _db.DavetiyeAyarlari.AsNoTracking().FirstOrDefaultAsync();
        return string.IsNullOrEmpty(ayar?.DavetUid) ? null : ayar.DavetUid;
    }

    public async Task EnsureDavetUidAsync()
    {
        const string fixedSlug = "24temmuz2026";
        var ayar = await _db.DavetiyeAyarlari.FirstOrDefaultAsync();
        if (ayar is null) return;

        // Sabit davetiye yolu; rastgele GUID üretilmez / mevcut rastgele değerler değiştirilir.
        if (string.Equals(ayar.DavetUid, fixedSlug, StringComparison.Ordinal))
            return;

        ayar.DavetUid = fixedSlug;
        await _db.SaveChangesAsync();
    }

    public async Task<string?> GetPanelUidAsync()
    {
        var ayar = await _db.DavetiyeAyarlari.AsNoTracking().FirstOrDefaultAsync();
        return string.IsNullOrEmpty(ayar?.PanelUid) ? null : ayar.PanelUid;
    }

    public async Task EnsurePanelUidAsync()
    {
        var ayar = await _db.DavetiyeAyarlari.FirstOrDefaultAsync();
        if (ayar is null || !string.IsNullOrEmpty(ayar.PanelUid))
            return;

        ayar.PanelUid = Guid.NewGuid().ToString("N");
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAyarlariAsync(DavetiyeAyarlari ayarlar)
    {
        _db.DavetiyeAyarlari.Update(ayarlar);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<TimelineOgesi>> GetTimelineAsync() =>
        await _db.TimelineOgeleri.OrderBy(t => t.Sira).ToListAsync();

    public async Task ReplaceTimelineAsync(IEnumerable<TimelineOgesi> ogeler)
    {
        _db.TimelineOgeleri.RemoveRange(_db.TimelineOgeleri);
        await _db.SaveChangesAsync();
        await _db.TimelineOgeleri.AddRangeAsync(ogeler);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<GaleriResmi>> GetGaleriAsync() =>
        await _db.GaleriResimleri.OrderBy(g => g.Sira).ToListAsync();

    public Task<GaleriResmi?> GetGaleriResmiByIdAsync(int id) =>
        _db.GaleriResimleri.FirstOrDefaultAsync(g => g.Id == id);

    public Task<int> CountGuestUploadsSinceAsync(DateTime sinceUtc, string uploadUrlPrefix)
    {
        var prefix = uploadUrlPrefix.TrimEnd('/') + "/";
        return _db.GaleriResimleri
            .Where(g => g.Url.StartsWith(prefix) && g.YuklemeTarihi >= sinceUtc)
            .CountAsync();
    }

    public async Task SetGaleriOnayAsync(int id, bool onaylandi)
    {
        var item = await _db.GaleriResimleri.FindAsync(id);
        if (item is null) return;
        item.Onaylandi = onaylandi;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetNextGaleriSiraAsync()
    {
        var max = await _db.GaleriResimleri.MaxAsync(g => (int?)g.Sira);
        return (max ?? 0) + 1;
    }

    public async Task AddGaleriResimleriAsync(IEnumerable<GaleriResmi> resimler)
    {
        await _db.GaleriResimleri.AddRangeAsync(resimler);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteGaleriResmiAsync(int id)
    {
        var item = await _db.GaleriResimleri.FindAsync(id);
        if (item is null) return;
        _db.GaleriResimleri.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ReplaceGaleriAsync(IEnumerable<GaleriResmi> resimler)
    {
        _db.GaleriResimleri.RemoveRange(_db.GaleriResimleri);
        await _db.SaveChangesAsync();
        await _db.GaleriResimleri.AddRangeAsync(resimler);
        await _db.SaveChangesAsync();
    }
}

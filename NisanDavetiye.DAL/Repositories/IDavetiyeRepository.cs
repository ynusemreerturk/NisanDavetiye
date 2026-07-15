using NisanDavetiye.DAL.Entities;

namespace NisanDavetiye.DAL.Repositories;

public interface IDavetiyeRepository
{
    Task<DavetiyeAyarlari?> GetAyarlariAsync();
    Task<DavetiyeAyarlari?> GetAyarlariByUidAsync(string uid);
    Task<string?> GetDavetUidAsync();
    Task EnsureDavetUidAsync();
    Task<string?> GetPanelUidAsync();
    Task EnsurePanelUidAsync();
    Task UpdateAyarlariAsync(DavetiyeAyarlari ayarlar);
    Task<IReadOnlyList<TimelineOgesi>> GetTimelineAsync();
    Task ReplaceTimelineAsync(IEnumerable<TimelineOgesi> ogeler);
    Task<IReadOnlyList<GaleriResmi>> GetGaleriAsync();
    Task<GaleriResmi?> GetGaleriResmiByIdAsync(int id);
    Task<int> CountGuestUploadsSinceAsync(DateTime sinceUtc, string uploadUrlPrefix);
    Task SetGaleriOnayAsync(int id, bool onaylandi);
    Task SetGaleriDriveFileIdAsync(int id, string driveFileId);
    Task SetGaleriDriveFileIdsAsync(IReadOnlyList<(int Id, string DriveFileId)> updates);
    Task<IReadOnlyList<GaleriResmi>> GetGuestUploadsPendingDriveAsync(string uploadUrlPrefix);
    Task<int> GetNextGaleriSiraAsync();
    Task AddGaleriResimleriAsync(IEnumerable<GaleriResmi> resimler);
    Task DeleteGaleriResmiAsync(int id);
    Task ReplaceGaleriAsync(IEnumerable<GaleriResmi> resimler);
}

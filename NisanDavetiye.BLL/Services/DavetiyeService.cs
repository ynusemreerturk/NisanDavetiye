using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.DAL.Entities;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public class DavetiyeService : IDavetiyeService
{
    private readonly IDavetiyeRepository _repo;
    private readonly IMediaUrlSigner _mediaSigner;

    public DavetiyeService(IDavetiyeRepository repo, IMediaUrlSigner mediaSigner)
    {
        _repo = repo;
        _mediaSigner = mediaSigner;
    }

    public async Task<DavetiyeDto> GetDavetiyeByUidAsync(string uid)
    {
        if (!InviteSlug.IsValid(uid))
            throw new KeyNotFoundException();

        var ayar = await _repo.GetAyarlariByUidAsync(uid.Trim())
            ?? throw new KeyNotFoundException();

        var timeline = await _repo.GetTimelineAsync();
        var galeri = await _repo.GetGaleriAsync();

        return MapPublic(ayar, timeline, galeri);
    }

    public async Task<DavetiyeAdminDto> GetDavetiyeForAdminAsync()
    {
        var ayar = await _repo.GetAyarlariAsync()
            ?? throw new InvalidOperationException("Davetiye ayarları bulunamadı.");

        var timeline = await _repo.GetTimelineAsync();
        var galeri = await _repo.GetGaleriAsync();

        return MapAdmin(ayar, timeline, galeri);
    }

    public async Task<DavetiyeAdminDto> GuncelleAsync(DavetiyeGuncelleDto dto)
    {
        var ayar = await _repo.GetAyarlariAsync()
            ?? throw new InvalidOperationException("Davetiye ayarları bulunamadı.");

        ayar.GelinAdi = dto.GelinAdi.Trim();
        ayar.DamatAdi = dto.DamatAdi.Trim();
        ayar.BasHarpler = dto.BasHarpler.Trim();
        ayar.Baslik = dto.Baslik.Trim();
        ayar.HosgeldinMetni = dto.HosgeldinMetni.Trim();
        ayar.EtkinlikTarihi = dto.EtkinlikTarihi;
        ayar.MekanAdi = dto.MekanAdi.Trim();
        ayar.Adres = dto.Adres.Trim();
        ayar.HaritaEmbedUrl = dto.HaritaEmbedUrl.Trim();
        ayar.HaritaLink = dto.HaritaLink.Trim();
        ayar.KapakGorselUrl = dto.KapakGorselUrl.Trim();
        ayar.CiftFotoUrl = dto.CiftFotoUrl.Trim();
        ayar.AcilisVideoUrl = dto.AcilisVideoUrl.Trim();
        ayar.MuzikUrl = dto.MuzikUrl.Trim();
        ayar.ZarfArkaPlanUrl = dto.ZarfArkaPlanUrl.Trim();
        ayar.GaleriDriveKlasorUrl = dto.GaleriDriveKlasorUrl.Trim();

        await _repo.UpdateAyarlariAsync(ayar);

        var timeline = await _repo.GetTimelineAsync();
        var galeri = await _repo.GetGaleriAsync();

        return MapAdmin(ayar, timeline, galeri);
    }

    private DavetiyeDto MapPublic(
        DavetiyeAyarlari ayar,
        IReadOnlyList<TimelineOgesi> timeline,
        IReadOnlyList<GaleriResmi> galeri) =>
        new(
            ayar.GelinAdi,
            ayar.DamatAdi,
            ayar.BasHarpler,
            ayar.Baslik,
            ayar.HosgeldinMetni,
            ayar.EtkinlikTarihi,
            ayar.MekanAdi,
            ayar.Adres,
            ayar.HaritaEmbedUrl,
            ayar.HaritaLink,
            ayar.KapakGorselUrl,
            ayar.CiftFotoUrl,
            ayar.AcilisVideoUrl,
            ayar.MuzikUrl,
            ayar.ZarfArkaPlanUrl,
            ayar.GaleriDriveKlasorUrl,
            timeline.Select(t => new TimelineDto(t.Id, t.Baslik, t.Aciklama, t.Saat, t.Sira)).ToList(),
            galeri
                .Where(g => !_mediaSigner.IsGuestUploadUrl(g.Url) || g.Onaylandi)
                .Select(g => MapGaleri(g, forAdmin: false))
                .ToList());

    private DavetiyeAdminDto MapAdmin(
        DavetiyeAyarlari ayar,
        IReadOnlyList<TimelineOgesi> timeline,
        IReadOnlyList<GaleriResmi> galeri) =>
        new(
            ayar.DavetUid,
            ayar.PanelUid,
            ayar.GelinAdi,
            ayar.DamatAdi,
            ayar.BasHarpler,
            ayar.Baslik,
            ayar.HosgeldinMetni,
            ayar.EtkinlikTarihi,
            ayar.MekanAdi,
            ayar.Adres,
            ayar.HaritaEmbedUrl,
            ayar.HaritaLink,
            ayar.KapakGorselUrl,
            ayar.CiftFotoUrl,
            ayar.AcilisVideoUrl,
            ayar.MuzikUrl,
            ayar.ZarfArkaPlanUrl,
            ayar.GaleriDriveKlasorUrl,
            timeline.Select(t => new TimelineDto(t.Id, t.Baslik, t.Aciklama, t.Saat, t.Sira)).ToList(),
            galeri.Select(g => MapGaleri(g, forAdmin: true)).ToList());

    private GaleriDto MapGaleri(GaleriResmi item, bool forAdmin)
    {
        var misafir = _mediaSigner.IsGuestUploadUrl(item.Url);
        var url = misafir
            ? _mediaSigner.SignGuestFile(_mediaSigner.TryGetFileName(item.Url)!, forAdmin)
            : item.Url;

        return new GaleriDto(item.Id, url, item.AltMetin, item.Sira, item.Onaylandi, misafir);
    }
}

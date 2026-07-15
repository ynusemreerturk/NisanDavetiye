using Microsoft.EntityFrameworkCore;
using NisanDavetiye.DAL.Data;

namespace NisanDavetiye.DAL;

public static class DavetiyeDataSeeder
{
    public static async Task SeedAsync(NisanDavetiyeDbContext db)
    {
        var ayar = await db.DavetiyeAyarlari.FirstOrDefaultAsync();
        if (ayar is null) return;

        if (!string.IsNullOrWhiteSpace(ayar.GelinAdi))
            return;

        ayar.DavetUid = "24temmuz2026";
        ayar.GelinAdi = "Ceren";
        ayar.DamatAdi = "Emre";
        ayar.BasHarpler = "C & E";
        ayar.EtkinlikTarihi = new DateTime(2026, 7, 24, 19, 0, 0);
        ayar.AcilisVideoUrl = "/assets/video/intro.mp4";
        ayar.KapakGorselUrl = "/assets/video/hero.mp4";
        ayar.ZarfArkaPlanUrl = "";
        ayar.MuzikUrl = "/assets/audio/ballerina.mp3";
        ayar.MekanAdi = "Winner's Davet Evi";
        ayar.Adres = "Cumhuriyet Mah. Malatya Cad. Hazar Dağlı Kavşağı, Merkez / Elazığ";
        ayar.HaritaLink = "https://maps.app.goo.gl/6peSAmVGAKMruPxo6";
        ayar.HaritaEmbedUrl =
            "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3062.886!2d39.1768327!3d38.666045!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4076c1bca9e9b6c1%3A0xf6d1059611873dee!2sWinner%27s%20Davet%20Evi!5e0!3m2!1str!2str!4v1";

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Excellence UI için eski HasData / klasik tema medya yollarını düzeltir.
    /// Yalnızca bilinen legacy default değerleri günceller; admin özel değerlerini ezmez.
    /// </summary>
    public static async Task EnsureExcellenceMediaPathsAsync(NisanDavetiyeDbContext db)
    {
        var ayar = await db.DavetiyeAyarlari.FirstOrDefaultAsync();
        if (ayar is null) return;

        var changed = false;

        if (IsLegacy(ayar.KapakGorselUrl, "/assets/images/kapak.jpg"))
        {
            ayar.KapakGorselUrl = "/assets/video/hero.mp4";
            changed = true;
        }

        if (IsLegacy(ayar.AcilisVideoUrl, "/assets/video/acilis.mp4"))
        {
            ayar.AcilisVideoUrl = "/assets/video/intro.mp4";
            changed = true;
        }

        if (IsLegacy(ayar.MuzikUrl, "/assets/audio/muzik.mp3"))
        {
            ayar.MuzikUrl = "/assets/audio/ballerina.mp3";
            changed = true;
        }

        if (IsLegacy(ayar.ZarfArkaPlanUrl, "/assets/images/zarf-arka.jpg"))
        {
            ayar.ZarfArkaPlanUrl = "";
            changed = true;
        }

        if (string.Equals(ayar.BasHarpler?.Trim(), "C & Es", StringComparison.Ordinal))
        {
            ayar.BasHarpler = "C & E";
            changed = true;
        }

        if (changed)
            await db.SaveChangesAsync();
    }

    private static bool IsLegacy(string? current, string legacy) =>
        string.Equals((current ?? string.Empty).Trim(), legacy, StringComparison.OrdinalIgnoreCase);
}

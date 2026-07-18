using Microsoft.EntityFrameworkCore;
using NisanDavetiye.DAL.Entities;

namespace NisanDavetiye.DAL.Data;

public class NisanDavetiyeDbContext : DbContext
{
    public NisanDavetiyeDbContext(DbContextOptions<NisanDavetiyeDbContext> options) : base(options)
    {
    }

    public DbSet<DavetiyeAyarlari> DavetiyeAyarlari => Set<DavetiyeAyarlari>();
    public DbSet<TimelineOgesi> TimelineOgeleri => Set<TimelineOgesi>();
    public DbSet<GaleriResmi> GaleriResimleri => Set<GaleriResmi>();
    public DbSet<RsvpKayit> RsvpKayitlari => Set<RsvpKayit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DavetiyeAyarlari>().HasData(new DavetiyeAyarlari
        {
            Id = 1,
            DavetUid = "24temmuz2026",
            GelinAdi = "Ceren",
            DamatAdi = "Emre",
            BasHarpler = "C & E",
            Baslik = "Nişanımıza Davetlisiniz",
            HosgeldinMetni = "Bu mutlu günümüzde sizleri de aramızda görmekten büyük mutluluk duyarız. Sevgi ve saygılarımızla.",
            EtkinlikTarihi = new DateTime(2026, 7, 24, 19, 30, 0),
            MekanAdi = "Winner's Davet Evi",
            Adres = "Cumhuriyet Mah. Malatya Cad. Hazar Dağlı Kavşağı, Merkez / Elazığ",
            HaritaEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3062.886!2d39.1768327!3d38.666045!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4076c1bca9e9b6c1%3A0xf6d1059611873dee!2sWinner%27s%20Davet%20Evi!5e0!3m2!1str!2str!4v1",
            HaritaLink = "https://maps.app.goo.gl/6peSAmVGAKMruPxo6",
            KapakGorselUrl = "/assets/video/hero.mp4",
            CiftFotoUrl = "/assets/images/cift.jpg",
            AcilisVideoUrl = "/assets/video/intro.mp4",
            MuzikUrl = "/assets/audio/ballerina.mp3",
            ZarfArkaPlanUrl = ""
        });

        modelBuilder.Entity<TimelineOgesi>().HasData(
            new TimelineOgesi { Id = 1, Baslik = "İsteme Merasimi", Aciklama = "Ailelerimizin geleneksel isteme merasimi.", Saat = "19:00", Sira = 1 },
            new TimelineOgesi { Id = 2, Baslik = "Nişan Töreni", Aciklama = "Nişan merasimimiz başlıyor.", Saat = "19:30", Sira = 2 },
            new TimelineOgesi { Id = 3, Baslik = "Akşam Yemeği", Aciklama = "Yemek ikramımız başlayacaktır.", Saat = "20:00", Sira = 3 },
            new TimelineOgesi { Id = 4, Baslik = "Müzik & Eğlence", Aciklama = "Gecemiz müzik ve eğlence eşliğinde devam edecektir.", Saat = "20:30", Sira = 4 }
        );

        modelBuilder.Entity<GaleriResmi>().HasData(
            new GaleriResmi { Id = 1, Url = "/assets/images/galeri-1.jpg", AltMetin = "Anı 1", Sira = 1, Onaylandi = true },
            new GaleriResmi { Id = 2, Url = "/assets/images/galeri-2.jpg", AltMetin = "Anı 2", Sira = 2, Onaylandi = true },
            new GaleriResmi { Id = 3, Url = "/assets/images/galeri-3.jpg", AltMetin = "Anı 3", Sira = 3, Onaylandi = true },
            new GaleriResmi { Id = 4, Url = "/assets/images/galeri-4.jpg", AltMetin = "Anı 4", Sira = 4, Onaylandi = true }
        );
    }
}

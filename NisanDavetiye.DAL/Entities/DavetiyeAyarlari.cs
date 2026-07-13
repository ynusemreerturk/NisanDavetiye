namespace NisanDavetiye.DAL.Entities;

public class DavetiyeAyarlari
{
    public int Id { get; set; } = 1;
    /// <summary>Gizli davetiye linki anahtarı (32 karakter hex).</summary>
    public string DavetUid { get; set; } = string.Empty;
    /// <summary>Gizli yönetim paneli linki anahtarı (32 karakter hex).</summary>
    public string PanelUid { get; set; } = string.Empty;
    public string GelinAdi { get; set; } = string.Empty;
    public string DamatAdi { get; set; } = string.Empty;
    public string BasHarpler { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public string HosgeldinMetni { get; set; } = string.Empty;
    public DateTime EtkinlikTarihi { get; set; }
    public string MekanAdi { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string HaritaEmbedUrl { get; set; } = string.Empty;
    public string HaritaLink { get; set; } = string.Empty;
    public string KapakGorselUrl { get; set; } = string.Empty;
    public string CiftFotoUrl { get; set; } = string.Empty;
    public string AcilisVideoUrl { get; set; } = string.Empty;
    public string MuzikUrl { get; set; } = string.Empty;
    public string ZarfArkaPlanUrl { get; set; } = string.Empty;
    public string GaleriDriveKlasorUrl { get; set; } = string.Empty;
}

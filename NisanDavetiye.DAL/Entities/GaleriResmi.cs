namespace NisanDavetiye.DAL.Entities;

public class GaleriResmi
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string AltMetin { get; set; } = string.Empty;
    public int Sira { get; set; }
    public bool Onaylandi { get; set; }
    public DateTime YuklemeTarihi { get; set; } = DateTime.UtcNow;
}

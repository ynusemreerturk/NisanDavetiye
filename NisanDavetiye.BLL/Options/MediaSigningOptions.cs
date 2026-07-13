namespace NisanDavetiye.BLL.Options;

public class MediaSigningOptions
{
    public const string SectionName = "MediaSigning";

    public string SigningKey { get; set; } = string.Empty;
    public int PublicUrlLifetimeHours { get; set; } = 24;
    public int AdminUrlLifetimeHours { get; set; } = 12;
}

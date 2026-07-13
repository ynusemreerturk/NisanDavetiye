namespace NisanDavetiye.BLL.Options;

public class GaleriStorageOptions
{
    public const string SectionName = "GaleriStorage";

    /// <summary>Relative to API content root unless already rooted.</summary>
    public string UploadDirectory { get; set; } = "uploads/galeri";

    public string PublicUrlPrefix { get; set; } = "/uploads/galeri";

    public int MaxDailyUploadCount { get; set; } = 100;

    /// <summary>Resolved absolute path; set during API startup.</summary>
    public string AbsoluteUploadDirectory { get; set; } = "";
}

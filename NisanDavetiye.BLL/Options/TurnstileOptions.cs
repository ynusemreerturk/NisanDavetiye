namespace NisanDavetiye.BLL.Options;

public class TurnstileOptions
{
    public const string SectionName = "Turnstile";

    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}

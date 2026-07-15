using System.Text.RegularExpressions;

namespace NisanDavetiye.BLL;

public static partial class InviteSlug
{
    /// <summary>Sabit davetiye URL slug'ı: /i/24temmuz2026</summary>
    public const string Fixed = "24temmuz2026";

    [GeneratedRegex("^[a-z0-9-]{3,64}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 64 && SlugPattern().IsMatch(value.Trim());
}

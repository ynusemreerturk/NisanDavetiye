using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.BLL.Security;

namespace NisanDavetiye.BLL.Services;

public class MediaUrlSigner : IMediaUrlSigner
{
    private readonly GaleriStorageOptions _storage;
    private readonly MediaSigningOptions _signing;

    public MediaUrlSigner(
        IOptions<GaleriStorageOptions> storageOptions,
        IOptions<MediaSigningOptions> signingOptions)
    {
        _storage = storageOptions.Value;
        _signing = signingOptions.Value;
    }

    public bool IsGuestUploadUrl(string url)
    {
        var prefix = _storage.PublicUrlPrefix.TrimEnd('/');
        return url.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    public string? TryGetFileName(string url)
    {
        if (!IsGuestUploadUrl(url))
            return null;

        var prefix = _storage.PublicUrlPrefix.TrimEnd('/');
        var fileName = Path.GetFileName(url[(prefix.Length + 1)..]);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }

    public string SignGuestFile(string fileName, bool forAdmin)
    {
        if (string.IsNullOrWhiteSpace(_signing.SigningKey))
            throw new InvalidOperationException("Media imzalama anahtarı yapılandırılmamış.");

        var hours = forAdmin ? _signing.AdminUrlLifetimeHours : _signing.PublicUrlLifetimeHours;
        var exp = DateTimeOffset.UtcNow.AddHours(hours).ToUnixTimeSeconds();
        var sig = ComputeSignature(fileName, exp);
        var encoded = Uri.EscapeDataString(fileName);
        return $"/api/media/galeri/{encoded}?exp={exp}&sig={sig}";
    }

    public bool TryValidate(string fileName, long expUnix, string signature)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(signature))
            return false;

        if (expUnix < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        if (string.IsNullOrWhiteSpace(_signing.SigningKey))
            return false;

        var expected = ComputeSignature(fileName, expUnix);
        return SecureCompare.Equals(expected, signature);
    }

    private string ComputeSignature(string fileName, long expUnix)
    {
        var payload = $"{fileName}|{expUnix}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signing.SigningKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

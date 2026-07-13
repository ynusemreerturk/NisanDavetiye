using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/media/galeri")]
public class MediaController : ControllerBase
{
    private readonly IMediaUrlSigner _signer;
    private readonly GaleriStorageOptions _storage;

    public MediaController(IMediaUrlSigner signer, IOptions<GaleriStorageOptions> storage)
    {
        _signer = signer;
        _storage = storage.Value;
    }

    [HttpGet("{fileName}")]
    public IActionResult GetGaleriFile(string fileName, [FromQuery] long exp, [FromQuery] string sig)
    {
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName) || safeName != fileName)
            return NotFound();

        if (!_signer.TryValidate(safeName, exp, sig))
            return NotFound();

        var diskPath = Path.Combine(_storage.AbsoluteUploadDirectory, safeName);
        if (!System.IO.File.Exists(diskPath))
            return NotFound();

        var contentType = ContentTypeFromExtension(Path.GetExtension(safeName));
        return PhysicalFile(diskPath, contentType);
    }

    private static string ContentTypeFromExtension(string ext) =>
        ext.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => "application/octet-stream",
        };
}

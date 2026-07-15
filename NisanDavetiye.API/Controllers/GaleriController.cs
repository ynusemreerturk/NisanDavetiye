using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequestSizeLimit(160_000_000)]
public class GaleriController : ControllerBase
{
    private readonly IGaleriService _service;
    private readonly ICaptchaService _captcha;

    public GaleriController(IGaleriService service, ICaptchaService captcha)
    {
        _service = service;
        _captcha = captcha;
    }

    [HttpPost("upload")]
    [EnableRateLimiting("public-forms")]
    [RequestFormLimits(MultipartBodyLengthLimit = 160_000_000)]
    public async Task<ActionResult<GaleriUploadResultDto>> Upload(
        [FromForm] List<IFormFile> files,
        [FromForm] string captchaToken,
        CancellationToken cancellationToken)
    {
        var ownedStreams = new List<Stream>();
        try
        {
            await _captcha.ValidateOrThrowAsync(
                captchaToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            var uploads = new List<GaleriUploadFile>();
            foreach (var file in files.Where(f => f.Length > 0))
            {
                // Belleğe tüm dosyayı kopyalama; isteğin stream'ini doğrudan ilet.
                var stream = file.OpenReadStream();
                ownedStreams.Add(stream);
                uploads.Add(new GaleriUploadFile(
                    file.FileName,
                    file.ContentType,
                    stream,
                    file.Length));
            }

            var result = await _service.UploadAsync(uploads, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
        finally
        {
            foreach (var stream in ownedStreams)
                await stream.DisposeAsync();
        }
    }
}

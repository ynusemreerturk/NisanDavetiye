using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RsvpController : ControllerBase
{
    private readonly IRsvpService _service;
    private readonly ICaptchaService _captcha;

    public RsvpController(IRsvpService service, ICaptchaService captcha)
    {
        _service = service;
        _captcha = captcha;
    }

    [HttpPost]
    [EnableRateLimiting("public-forms")]
    public async Task<ActionResult<RsvpDto>> Kaydet([FromBody] RsvpOlusturDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _captcha.ValidateOrThrowAsync(
                dto.CaptchaToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            return Ok(await _service.KaydetAsync(dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
    }
}

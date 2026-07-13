using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly TurnstileOptions _turnstile;

    public ConfigController(IOptions<TurnstileOptions> turnstile) => _turnstile = turnstile.Value;

    [HttpGet("client")]
    public IActionResult GetClientConfig() =>
        Ok(new
        {
            turnstileSiteKey = _turnstile.Enabled ? _turnstile.SiteKey : string.Empty,
            turnstileEnabled = _turnstile.Enabled && !string.IsNullOrWhiteSpace(_turnstile.SiteKey),
        });
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NisanDavetiye.BLL.Options;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly RecaptchaOptions _recaptcha;

    public ConfigController(IOptions<RecaptchaOptions> recaptcha) => _recaptcha = recaptcha.Value;

    [HttpGet("client")]
    public IActionResult GetClientConfig() =>
        Ok(new
        {
            recaptchaSiteKey = _recaptcha.Enabled ? _recaptcha.SiteKey : string.Empty,
            recaptchaEnabled = _recaptcha.Enabled && !string.IsNullOrWhiteSpace(_recaptcha.SiteKey),
        });
}

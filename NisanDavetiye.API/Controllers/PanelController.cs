using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/panel")]
[EnableRateLimiting("panel-api")]
public class PanelController : ControllerBase
{
    private readonly IDavetiyeService _davetiyeService;
    private readonly IPanelAuthService _panelAuth;

    public PanelController(IDavetiyeService davetiyeService, IPanelAuthService panelAuth)
    {
        _davetiyeService = davetiyeService;
        _panelAuth = panelAuth;
    }

    [HttpGet("access/{uid}")]
    [EnableRateLimiting("panel-access")]
    public async Task<IActionResult> ValidateAccess(string uid)
    {
        if (!await _panelAuth.IsValidPanelUidAsync(uid))
            return NotFound(new { message = "Geçersiz yönetim bağlantısı." });

        return Ok(new { ok = true });
    }

    [HttpGet("davetiye")]
    public async Task<ActionResult<DavetiyeAdminDto>> GetForAdmin() =>
        Ok(await _davetiyeService.GetDavetiyeForAdminAsync());

    [HttpPut("davetiye")]
    public async Task<ActionResult<DavetiyeAdminDto>> Guncelle([FromBody] DavetiyeGuncelleDto dto) =>
        Ok(await _davetiyeService.GuncelleAsync(dto));
}

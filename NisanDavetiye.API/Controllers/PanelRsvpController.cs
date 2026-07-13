using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/panel/rsvp")]
[EnableRateLimiting("panel-api")]
public class PanelRsvpController : ControllerBase
{
    private readonly IRsvpService _service;

    public PanelRsvpController(IRsvpService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RsvpDto>>> Listele() =>
        Ok(await _service.ListeleAsync());

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Gizle(int id)
    {
        await _service.GizleFromAdminAsync(id);
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var bytes = await _service.ExcelExportAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"katilim-yanitlari-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }
}

using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DavetiyeController : ControllerBase
{
    private readonly IDavetiyeService _service;

    public DavetiyeController(IDavetiyeService service) => _service = service;

    [HttpGet("{uid}")]
    public async Task<ActionResult<DavetiyeDto>> GetByUid(string uid)
    {
        try
        {
            return Ok(await _service.GetDavetiyeByUidAsync(uid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Davetiye bulunamadı." });
        }
    }
}

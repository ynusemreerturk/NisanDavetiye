using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Services;

namespace NisanDavetiye.API.Controllers;

[ApiController]
[Route("api/panel/galeri")]
[EnableRateLimiting("panel-api")]
public class PanelGaleriController : ControllerBase
{
    private readonly IGaleriService _service;

    public PanelGaleriController(IGaleriService service) => _service = service;

    [HttpGet("export")]
    public async Task<IActionResult> ExportZip(CancellationToken cancellationToken)
    {
        try
        {
            var (content, fileName) = await _service.ExportUploadedZipAsync(cancellationToken);
            return File(content, "application/zip", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("uploaded")]
    public async Task<ActionResult<GaleriSilResultDto>> DeleteAllUploaded()
    {
        var result = await _service.DeleteAllUploadedPhotosAsync();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<GaleriSilResultDto>> Delete(int id)
    {
        try
        {
            return Ok(await _service.DeleteUploadedPhotoAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<GaleriDto>> Approve(int id)
    {
        try
        {
            return Ok(await _service.ApproveUploadedPhotoAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<GaleriSilResultDto>> Reject(int id)
    {
        try
        {
            return Ok(await _service.RejectUploadedPhotoAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _managerService;

    public ManagerController(IManagerService managerService)
    {
        _managerService = managerService;
    }

    [HttpPost]
    public async Task<ActionResult<ManagerDto>> CreateManager([FromBody] CreateManagerRequest request)
    {
        try
        {
            var manager = await _managerService.CreateManagerAsync(request);
            return CreatedAtAction(nameof(GetManager), new { id = manager.Id }, manager);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ManagerDto>>> GetAllManagers()
    {
        try
        {
            var managers = await _managerService.GetAllManagersAsync();
            return Ok(managers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManagerDto>> GetManager(Guid id)
    {
        try
        {
            var manager = await _managerService.GetManagerByIdAsync(id);
            if (manager == null)
                return NotFound(new { message = "Manager not found" });

            return Ok(manager);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateManager(Guid id, [FromBody] CreateManagerRequest request)
    {
        try
        {
            var result = await _managerService.UpdateManagerAsync(id, request);
            if (!result)
                return NotFound(new { message = "Manager not found" });

            return Ok(new { message = "Manager updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeactivateManager(Guid id)
    {
        try
        {
            var result = await _managerService.DeactivateManagerAsync(id);
            if (!result)
                return NotFound(new { message = "Manager not found" });

            return Ok(new { message = "Manager deactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}


using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/servicetypes")]
[AllowAnonymous] // Allow anonymous access for product listing
public class ServiceTypeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceTypeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceTypeDto>>> GetServiceTypes([FromQuery] bool activeOnly = true)
    {
        var query = _context.ServiceTypes.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var serviceTypes = await query
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceTypeDto
            {
                Id = s.Id,
                ServiceCode = s.ServiceCode,
                ServiceName = s.ServiceName,
                Description = s.Description,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Ok(serviceTypes);
    }
}

public class ServiceTypeDto
{
    public Guid Id { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}


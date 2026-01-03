using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/company")]
public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompanyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous] // Allow anonymous access for company list (needed for registration form)
    public async Task<ActionResult<List<CompanyDto>>> GetCompanies([FromQuery] bool activeOnly = true)
    {
        var query = _context.Companies.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var companies = await query
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(companies);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")] // Only Managers can create companies
    public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Company name is required" });

        var trimmedName = request.Name.Trim();

        // Check if company already exists (case-insensitive)
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == trimmedName.ToLower());

        if (existingCompany != null)
        {
            if (existingCompany.IsActive)
                return Conflict(new { message = $"Company '{trimmedName}' already exists" });
            else
                return Conflict(new { message = $"Company '{trimmedName}' exists but is inactive" });
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        var companyDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };

        return Created($"/api/company/{company.Id}", companyDto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        
        if (company == null)
            return NotFound(new { message = "Company not found" });

        var companyDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };

        return Ok(companyDto);
    }
}


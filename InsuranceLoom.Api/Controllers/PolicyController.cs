using InsuranceLoom.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/policy")]
[Authorize]
public class PolicyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PolicyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("broker/{brokerId}")]
    [Authorize(Roles = "Broker")]
    public async Task<IActionResult> GetBrokerPolicies(Guid brokerId)
    {
        try
        {
            // Verify the broker ID matches the authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var broker = await _context.Brokers
                .FirstOrDefaultAsync(b => b.Id == brokerId && b.UserId == userId);

            if (broker == null)
            {
                return Unauthorized(new { message = "Broker not found or access denied" });
            }

            var policies = await _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.ServiceType)
                .Where(p => p.BrokerId == brokerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    id = p.Id,
                    policyNumber = p.PolicyNumber,
                    policyHolderName = $"{p.PolicyHolder!.FirstName} {p.PolicyHolder.LastName}",
                    serviceType = p.ServiceType!.ServiceName,
                    coverageAmount = p.CoverageAmount,
                    premiumAmount = p.PremiumAmount,
                    status = p.Status,
                    startDate = p.StartDate,
                    createdAt = p.CreatedAt,
                    updatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(policies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching policies", error = ex.Message });
        }
    }
}


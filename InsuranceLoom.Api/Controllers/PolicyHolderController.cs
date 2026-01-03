using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using InsuranceLoom.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/policyholder")]
[Authorize]
public class PolicyHolderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public PolicyHolderController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<PolicyHolderDto>> RegisterPolicyHolder([FromBody] CreatePolicyHolderRequest request)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Get broker ID from authenticated user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.User != null && b.User.Email == userEmail);

            if (broker == null || !broker.IsActive)
            {
                return Unauthorized(new { message = "Broker not found or inactive" });
            }

            // Generate unique policy number
            string policyNumber = await GenerateUniquePolicyNumberAsync();

            // Create user account
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                UserType = "PolicyHolder",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Create policy holder
            var policyHolder = new PolicyHolder
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PolicyNumber = policyNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IdNumber = request.IdNumber,
                Phone = request.Phone,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PolicyHolders.Add(policyHolder);

            // Create policy if service type is provided
            Policy? policy = null;
            if (request.ServiceTypeId.HasValue)
            {
                var serviceType = await _context.ServiceTypes.FindAsync(request.ServiceTypeId.Value);
                if (serviceType == null || !serviceType.IsActive)
                {
                    return BadRequest(new { message = "Invalid service type" });
                }

                policy = new Policy
                {
                    Id = Guid.NewGuid(),
                    PolicyNumber = await GenerateUniquePolicyNumberAsync(),
                    PolicyHolderId = policyHolder.Id,
                    BrokerId = broker.Id,
                    ServiceTypeId = request.ServiceTypeId.Value,
                    ServiceCode = serviceType.ServiceCode,
                    CoverageAmount = request.CoverageAmount,
                    PremiumAmount = request.PremiumAmount,
                    StartDate = request.StartDate ?? DateTime.UtcNow,
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Policies.Add(policy);
            }

            await _context.SaveChangesAsync();

            return Ok(new PolicyHolderDto
            {
                Id = policyHolder.Id,
                PolicyNumber = policyHolder.PolicyNumber,
                FirstName = policyHolder.FirstName,
                LastName = policyHolder.LastName,
                Email = user.Email,
                Phone = policyHolder.Phone,
                IdNumber = policyHolder.IdNumber,
                Address = policyHolder.Address,
                DateOfBirth = policyHolder.DateOfBirth,
                IsActive = policyHolder.IsActive,
                CreatedAt = policyHolder.CreatedAt,
                PolicyId = policy?.Id,
                PolicyNumberGenerated = policy?.PolicyNumber
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the policy holder", error = ex.Message });
        }
    }

    private async Task<string> GenerateUniquePolicyNumberAsync()
    {
        string policyNumber;
        bool isUnique = false;
        int attempts = 0;
        const int maxAttempts = 10;

        while (!isUnique && attempts < maxAttempts)
        {
            policyNumber = $"POL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            var exists = await _context.PolicyHolders.AnyAsync(p => p.PolicyNumber == policyNumber);
            if (!exists)
            {
                return policyNumber;
            }
            attempts++;
        }

        // Fallback with timestamp
        return $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(10000, 99999)}";
    }
}

public class CreatePolicyHolderRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime? StartDate { get; set; }
}

public class PolicyHolderDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyNumberGenerated { get; set; }
}


using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Helpers;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/auth/client")]
public class ClientAuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public ClientAuthController(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] ClientRegisterRequest request)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

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

            // Create policy holder record (without policy number yet - will be assigned when application is submitted)
            var policyHolder = new PolicyHolder
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PolicyNumber = "", // Will be generated when first policy is created
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PolicyHolders.Add(policyHolder);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    userType = user.UserType
                },
                policyHolder = new
                {
                    id = policyHolder.Id,
                    firstName = policyHolder.FirstName,
                    lastName = policyHolder.LastName
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] ClientLoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.UserType == "PolicyHolder");

            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Your account is inactive. Please contact support." });
            }

            // Get policy holder info
            var policyHolder = await _context.PolicyHolders
                .FirstOrDefaultAsync(ph => ph.UserId == user.Id);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    userType = user.UserType
                },
                policyHolder = policyHolder != null ? new
                {
                    id = policyHolder.Id,
                    firstName = policyHolder.FirstName,
                    lastName = policyHolder.LastName,
                    policyNumber = policyHolder.PolicyNumber
                } : null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "InsuranceLoom";
        var audience = jwtSettings["Audience"] ?? "InsuranceLoom";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440"); // 24 hours default

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class ClientRegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class ClientLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


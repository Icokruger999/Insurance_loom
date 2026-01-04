using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Helpers;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, ApplicationDbContext context, IEmailService emailService)
    {
        _authService = authService;
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("broker/register")]
    public async Task<ActionResult<BrokerDto>> RegisterBroker([FromBody] CreateBrokerRequest request)
    {
        try
        {
            var broker = await _authService.RegisterBrokerAsync(request);
            return Created($"/api/broker/{broker.Id}", broker);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
        }
    }

    [HttpPost("broker/login")]
    public async Task<ActionResult<LoginResponse>> BrokerLogin([FromBody] BrokerLoginRequest request)
    {
        try
        {
            var response = await _authService.BrokerLoginAsync(request);
            if (response == null)
            {
                // Check if broker exists but is not approved
                var broker = await _context.Brokers
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.User != null && b.User.Email == request.Email);
                
                if (broker == null)
                    return Unauthorized(new { message = "No account found with this email address. Please register first." });
                
                if (broker.User != null && !broker.User.IsActive)
                    return Unauthorized(new { message = "Your account has been deactivated. Please contact support." });
                
                if (!broker.IsActive)
                    return Unauthorized(new { message = "Your account is pending approval. Please wait for manager approval before logging in. You will receive an email when approved." });
                
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("policyholder/login")]
    public async Task<ActionResult<LoginResponse>> PolicyHolderLogin([FromBody] PolicyHolderLoginRequest request)
    {
        try
        {
            var response = await _authService.PolicyHolderLoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid policy number or password" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("manager/login")]
    public async Task<ActionResult<LoginResponse>> ManagerLogin([FromBody] ManagerLoginRequest request)
    {
        try
        {
            var response = await _authService.ManagerLoginAsync(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("manager/forgot-password")]
    public async Task<ActionResult> ManagerForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var manager = await _context.Managers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.User != null && m.User.Email == request.Email);

            if (manager == null || manager.User == null)
            {
                // Don't reveal if email exists for security
                return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
            }

            // Generate a temporary password
            var tempPassword = Guid.NewGuid().ToString("N")[..12]; // 12 character temporary password
            
            // Hash the temporary password
            manager.User.PasswordHash = PasswordHasher.HashPassword(tempPassword);

            await _context.SaveChangesAsync();

            // Send email with temporary password
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {manager.FirstName} {manager.LastName},</p>
                <p>You have requested to reset your password for your Insurance Loom Manager account.</p>
                <p><strong>Your temporary password is: {tempPassword}</strong></p>
                <p>Please log in with this temporary password and change it immediately after logging in.</p>
                <p>If you did not request this password reset, please contact support immediately.</p>
                <p>Best regards,<br>Insurance Loom Team</p>
            ";

            await _emailService.SendEmailAsync(
                manager.User.Email,
                "Password Reset - Insurance Loom",
                emailBody
            );

            return Ok(new { message = "Password reset email has been sent. Please check your inbox." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    [HttpPost("broker/forgot-password")]
    public async Task<ActionResult> BrokerForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.User != null && b.User.Email == request.Email);

            if (broker == null || broker.User == null)
            {
                return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
            }

            var tempPassword = Guid.NewGuid().ToString("N")[..12];
            broker.User.PasswordHash = PasswordHasher.HashPassword(tempPassword);
            await _context.SaveChangesAsync();

            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {broker.FirstName} {broker.LastName},</p>
                <p>You have requested to reset your password for your Insurance Loom Agent account.</p>
                <p><strong>Your temporary password is: {tempPassword}</strong></p>
                <p>Please log in with this temporary password and change it immediately after logging in.</p>
                <p>If you did not request this password reset, please contact support immediately.</p>
                <p>Best regards,<br>Insurance Loom Team</p>
            ";

            await _emailService.SendEmailAsync(
                broker.User.Email,
                "Password Reset - Insurance Loom",
                emailBody
            );

            return Ok(new { message = "Password reset email has been sent. Please check your inbox." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    [HttpPost("manager/change-password")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> ManagerChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var manager = await _context.Managers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.User != null && m.User.Id == userId);

            if (manager == null || manager.User == null)
            {
                return NotFound(new { message = "Manager not found" });
            }

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, manager.User.PasswordHash))
            {
                return Unauthorized(new { message = "Current password is incorrect" });
            }

            manager.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            manager.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while changing password", error = ex.Message });
        }
    }

    [HttpPost("broker/change-password")]
    [Authorize(Roles = "Broker")]
    public async Task<ActionResult> BrokerChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.User != null && b.User.Id == userId);

            if (broker == null || broker.User == null)
            {
                return NotFound(new { message = "Broker not found" });
            }

            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, broker.User.PasswordHash))
            {
                return Unauthorized(new { message = "Current password is incorrect" });
            }

            broker.User.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            broker.User.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while changing password", error = ex.Message });
        }
    }
}


using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/broker")]
public class BrokerApprovalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public BrokerApprovalController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("{brokerId}/approve")]
    [HttpPost("{brokerId}/approve")]
    public async Task<IActionResult> ApproveBroker(Guid brokerId)
    {
        try
        {
            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == brokerId);

            if (broker == null)
            {
                var notFoundHtml = @"<html><head><title>Broker Not Found</title></head><body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;""><h1>Broker Not Found</h1><p>The broker you're trying to approve does not exist.</p></body></html>";
                return Content(notFoundHtml, "text/html");
            }

            if (broker.IsActive)
            {
                var alreadyApprovedHtml = $@"<html><head><title>Already Approved</title></head><body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;""><h1>Already Approved</h1><p>Broker {broker.FirstName} {broker.LastName} ({broker.AgentNumber}) is already approved.</p><p><a href=""https://www.insuranceloom.com"">Return to Insurance Loom</a></p></body></html>";
                return Content(alreadyApprovedHtml, "text/html");
            }

            broker.IsActive = true;
            broker.UpdatedAt = DateTime.UtcNow;
            if (broker.User != null)
            {
                broker.User.IsActive = true;
                broker.User.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            // Send approval notification email to broker
            if (broker.User != null)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        broker.User.Email,
                        "Insurance Loom - Your Broker Account Has Been Approved",
                        $@"
                            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
                                <div style=""background-color: #1a1a1a; color: #ffffff; padding: 20px; text-align: center;"">
                                    <h1 style=""margin: 0;"">Insurance Loom</h1>
                                </div>
                                <div style=""background-color: #ffffff; padding: 30px;"">
                                    <h2 style=""color: #333333;"">Account Approved!</h2>
                                    <p style=""color: #666666; line-height: 1.6;"">Good news, {broker.FirstName}! Your broker account has been approved.</p>
                                    <div style=""background-color: #f8f9fa; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;"">
                                        <p style=""margin: 0; color: #333333;""><strong>Your Agent Number:</strong> <span style=""font-size: 18px; color: #1a1a1a;"">{broker.AgentNumber}</span></p>
                                    </div>
                                    <p style=""color: #666666; line-height: 1.6;"">You can now log in to the Insurance Loom portal using your email address and password.</p>
                                    <div style=""text-align: center; margin: 30px 0;"">
                                        <a href=""https://www.insuranceloom.com"" style=""display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px;"">Login to Your Account</a>
                                    </div>
                                    <p style=""color: #666666; line-height: 1.6; font-size: 14px; margin-top: 30px;"">Best regards,<br>The Insurance Loom Team</p>
                                </div>
                            </div>
                        "
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send approval email: {ex.Message}");
                }
            }

            // Return HTML page for browser display
            var successHtml = $@"<html>
<head>
    <title>Broker Approved</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #28a745; margin-bottom: 20px; }}
        p {{ color: #666; line-height: 1.6; margin: 15px 0; }}
        .info-box {{ background-color: #f8f9fa; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0; text-align: left; }}
        .btn {{ display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .btn:hover {{ background-color: #333; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>✓ Broker Approved Successfully</h1>
        <p>Broker <strong>{broker.FirstName} {broker.LastName}</strong> has been approved and can now log in.</p>
        <div class=""info-box"">
            <p><strong>Agent Number:</strong> {broker.AgentNumber}</p>
            <p><strong>Email:</strong> {broker.User?.Email ?? "N/A"}</p>
            <p><strong>Company:</strong> {broker.CompanyName ?? "N/A"}</p>
        </div>
        <p>An approval notification email has been sent to the broker.</p>
        <a href=""https://www.insuranceloom.com"" class=""btn"">Return to Insurance Loom</a>
    </div>
</body>
</html>";
            return Content(successHtml, "text/html");
        }
        catch (Exception ex)
        {
            var errorHtml = $@"<html><head><title>Error</title></head><body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;""><h1 style=""color: #dc3545;"">Error</h1><p>An error occurred during approval: {ex.Message}</p><p><a href=""https://www.insuranceloom.com"">Return to Insurance Loom</a></p></body></html>";
            return Content(errorHtml, "text/html");
        }
    }

    [HttpGet("{brokerId}/reject")]
    [HttpPost("{brokerId}/reject")]
    public async Task<IActionResult> RejectBroker(Guid brokerId, [FromBody] RejectBrokerRequest? request = null)
    {
        try
        {
            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == brokerId);

            if (broker == null)
            {
                var notFoundHtml = @"<html><head><title>Broker Not Found</title></head><body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;""><h1>Broker Not Found</h1><p>The broker you're trying to reject does not exist.</p><p><a href=""https://www.insuranceloom.com"">Return to Insurance Loom</a></p></body></html>";
                return Content(notFoundHtml, "text/html");
            }

            // Send rejection email before deleting
            if (broker.User != null)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        broker.User.Email,
                        "Insurance Loom - Broker Registration Status",
                        $@"
                            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
                                <div style=""background-color: #1a1a1a; color: #ffffff; padding: 20px; text-align: center;"">
                                    <h1 style=""margin: 0;"">Insurance Loom</h1>
                                </div>
                                <div style=""background-color: #ffffff; padding: 30px;"">
                                    <h2 style=""color: #333333;"">Registration Status</h2>
                                    <p style=""color: #666666; line-height: 1.6;"">Dear {broker.FirstName},</p>
                                    <p style=""color: #666666; line-height: 1.6;"">We regret to inform you that your broker registration has been rejected.</p>
                                    {(request?.Reason != null ? $@"<p style=""color: #666666; line-height: 1.6;""><strong>Reason:</strong> {request.Reason}</p>" : "")}
                                    <p style=""color: #666666; line-height: 1.6; font-size: 14px; margin-top: 30px;"">If you have any questions, please contact our support team.</p>
                                    <p style=""color: #666666; line-height: 1.6; font-size: 14px;"">Best regards,<br>The Insurance Loom Team</p>
                                </div>
                            </div>
                        "
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send rejection email: {ex.Message}");
                }

                _context.Users.Remove(broker.User);
            }

            await _context.SaveChangesAsync();

            // Return HTML page for browser display
            var rejectHtml = $@"<html>
<head>
    <title>Broker Rejected</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #dc3545; margin-bottom: 20px; }}
        p {{ color: #666; line-height: 1.6; margin: 15px 0; }}
        .btn {{ display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .btn:hover {{ background-color: #333; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>Broker Rejected</h1>
        <p>Broker <strong>{broker.FirstName} {broker.LastName}</strong> has been rejected and their account has been removed.</p>
        <p>A rejection notification email has been sent to the broker.</p>
        <a href=""https://www.insuranceloom.com"" class=""btn"">Return to Insurance Loom</a>
    </div>
</body>
</html>";
            return Content(rejectHtml, "text/html");
        }
        catch (Exception ex)
        {
            var errorHtml = $@"<html><head><title>Error</title></head><body style=""font-family: Arial, sans-serif; text-align: center; padding: 50px;""><h1 style=""color: #dc3545;"">Error</h1><p>An error occurred during rejection: {ex.Message}</p><p><a href=""https://www.insuranceloom.com"">Return to Insurance Loom</a></p></body></html>";
            return Content(errorHtml, "text/html");
        }
    }
}

public class RejectBrokerRequest
{
    public string? Reason { get; set; }
}


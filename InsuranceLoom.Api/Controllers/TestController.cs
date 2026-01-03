using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("email")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            await _emailService.SendEmailAsync(
                request.To,
                request.Subject ?? "Test Email from Insurance Loom API",
                request.Body ?? "<h1>Test Email</h1><p>This is a test email from Insurance Loom API.</p><p>If you received this, the email configuration is working correctly.</p>"
            );

            return Ok(new { message = "Test email sent successfully", to = request.To });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to send test email", error = ex.Message });
        }
    }
}

public class TestEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
}


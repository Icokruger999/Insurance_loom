using System.Net;
using System.Net.Mail;

namespace InsuranceLoom.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _username;
    private readonly string _password;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var emailSettings = configuration.GetSection("EmailSettings");
        _smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        _senderEmail = emailSettings["SenderEmail"] ?? "noreply@insuranceloom.com";
        _senderName = emailSettings["SenderName"] ?? "Insurance Loom";
        _username = emailSettings["Username"] ?? string.Empty;
        _password = emailSettings["Password"] ?? string.Empty;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_username, _password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Log error - in production, use proper logging
            Console.WriteLine($"Email send failed: {ex.Message}");
        }
    }

    public async Task SendPolicySubmittedNotificationAsync(string managerEmail, string policyNumber, string brokerName)
    {
        var subject = $"New Policy Approval Request - {policyNumber}";
        var body = $@"
            <h2>New Policy Submitted for Approval</h2>
            <p><strong>Policy Number:</strong> {policyNumber}</p>
            <p><strong>Submitted by:</strong> {brokerName}</p>
            <p>Please review and approve this policy in the Insurance Loom system.</p>
            <p><a href='https://insuranceloom.com/manager/approvals'>View Pending Approvals</a></p>
        ";

        await SendEmailAsync(managerEmail, subject, body);
    }

    public async Task SendPolicyApprovedNotificationAsync(string brokerEmail, string policyHolderEmail, string policyNumber)
    {
        var subject = $"Policy Approved - {policyNumber}";
        var body = $@"
            <h2>Policy Approved</h2>
            <p><strong>Policy Number:</strong> {policyNumber}</p>
            <p>Your policy has been approved and is now active.</p>
        ";

        await SendEmailAsync(brokerEmail, subject, body);
        await SendEmailAsync(policyHolderEmail, subject, body);
    }

    public async Task SendPolicyRejectedNotificationAsync(string brokerEmail, string policyNumber, string reason)
    {
        var subject = $"Policy Rejected - {policyNumber}";
        var body = $@"
            <h2>Policy Rejected</h2>
            <p><strong>Policy Number:</strong> {policyNumber}</p>
            <p><strong>Reason:</strong> {reason}</p>
            <p>Please review and resubmit with the necessary corrections.</p>
        ";

        await SendEmailAsync(brokerEmail, subject, body);
    }

    public async Task SendChangesRequestedNotificationAsync(string brokerEmail, string policyNumber, string changesRequired)
    {
        var subject = $"Policy Changes Required - {policyNumber}";
        var body = $@"
            <h2>Changes Required</h2>
            <p><strong>Policy Number:</strong> {policyNumber}</p>
            <p><strong>Required Changes:</strong></p>
            <p>{changesRequired}</p>
            <p>Please make the necessary changes and resubmit the policy.</p>
        ";

        await SendEmailAsync(brokerEmail, subject, body);
    }
}


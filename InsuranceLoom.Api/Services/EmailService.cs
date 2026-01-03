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

    public async Task SendBrokerRegistrationNotificationAsync(string brokerEmail, string agentNumber, string firstName, string lastName)
    {
        var subject = "Welcome to Insurance Loom - Registration Successful";
        var body = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
                <div style=""background-color: #1a1a1a; color: #ffffff; padding: 20px; text-align: center;"">
                    <h1 style=""margin: 0;"">Insurance Loom</h1>
                </div>
                <div style=""background-color: #ffffff; padding: 30px;"">
                    <h2 style=""color: #333333;"">Welcome, {firstName} {lastName}!</h2>
                    <p style=""color: #666666; line-height: 1.6;"">Thank you for registering with Insurance Loom. Your account has been successfully created.</p>
                    <div style=""background-color: #f8f9fa; padding: 20px; border-left: 4px solid #4a4a4a; margin: 20px 0;"">
                        <p style=""margin: 0; color: #333333;""><strong>Your Agent Number:</strong> <span style=""font-size: 18px; color: #1a1a1a;"">{agentNumber}</span></p>
                        <p style=""margin: 10px 0 0 0; color: #666666; font-size: 14px;"">Please save this number - you'll need it to log in to your account.</p>
                    </div>
                    <p style=""color: #666666; line-height: 1.6;"">You can now log in to the Insurance Loom portal using your Agent Number and password.</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""https://www.insuranceloom.com"" style=""display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px;"">Login to Your Account</a>
                    </div>
                    <p style=""color: #666666; line-height: 1.6; font-size: 14px;"">If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                    <p style=""color: #666666; line-height: 1.6; font-size: 14px; margin-top: 30px;"">Best regards,<br>The Insurance Loom Team</p>
                </div>
                <div style=""background-color: #1a1a1a; color: #a0a0a0; padding: 15px; text-align: center; font-size: 12px;"">
                    <p style=""margin: 0;"">© 2024 Insurance Loom. All rights reserved.</p>
                    <p style=""margin: 5px 0 0 0;"">This is an automated message. Please do not reply to this email.</p>
                </div>
            </div>
        ";

        await SendEmailAsync(brokerEmail, subject, body);
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


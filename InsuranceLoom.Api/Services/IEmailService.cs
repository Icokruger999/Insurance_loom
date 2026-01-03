namespace InsuranceLoom.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendBrokerRegistrationNotificationAsync(string brokerEmail, string agentNumber, string firstName, string lastName);
    Task SendBrokerApprovalRequestAsync(string approverEmail, string brokerEmail, string agentNumber, string firstName, string lastName, string companyName, Guid brokerId);
    Task SendPolicySubmittedNotificationAsync(string managerEmail, string policyNumber, string brokerName);
    Task SendPolicyApprovedNotificationAsync(string brokerEmail, string policyHolderEmail, string policyNumber);
    Task SendPolicyRejectedNotificationAsync(string brokerEmail, string policyNumber, string reason);
    Task SendChangesRequestedNotificationAsync(string brokerEmail, string policyNumber, string changesRequired);
}


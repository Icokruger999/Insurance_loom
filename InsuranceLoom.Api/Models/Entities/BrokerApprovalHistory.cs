namespace InsuranceLoom.Api.Models.Entities;

public class BrokerApprovalHistory
{
    public Guid Id { get; set; }
    public Guid BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public string Action { get; set; } = string.Empty; // Registered, Approved, Rejected
    public Guid? PerformedByManagerId { get; set; }
    public Manager? PerformedByManager { get; set; }
    public string? PerformedByEmail { get; set; } // Store email in case manager is deleted
    public string? Notes { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


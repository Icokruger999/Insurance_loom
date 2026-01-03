namespace InsuranceLoom.Api.Models.Entities;

public class PolicyApprovalHistory
{
    public Guid Id { get; set; }
    public Guid ApprovalId { get; set; }
    public PolicyApproval? Approval { get; set; }
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public string Action { get; set; } = string.Empty; // Submitted, Assigned, Reviewed, Approved, Rejected, ChangesRequested
    public Guid? PerformedBy { get; set; }
    public string? PerformedByType { get; set; } // Broker, Manager
    public string? Notes { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


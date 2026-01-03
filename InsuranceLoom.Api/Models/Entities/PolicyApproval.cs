namespace InsuranceLoom.Api.Models.Entities;

public class PolicyApproval
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public Guid BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, UnderReview, Approved, Rejected, RequiresChanges
    public Guid? SubmittedBy { get; set; }
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public Guid? AssignedManagerId { get; set; }
    public Manager? AssignedManager { get; set; }
    public DateTime? AssignedDate { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNotes { get; set; }
    public bool DocumentsVerified { get; set; } = false;
    public Guid? DocumentsVerifiedBy { get; set; }
    public DateTime? DocumentsVerifiedDate { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ChangesRequestedBy { get; set; }
    public DateTime? ChangesRequestedDate { get; set; }
    public string? ChangesRequired { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


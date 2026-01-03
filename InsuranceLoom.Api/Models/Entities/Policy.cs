namespace InsuranceLoom.Api.Models.Entities;

public class Policy
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public Guid BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public Guid ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }
    public string? ServiceCode { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, PendingSubmission, Submitted, UnderReview, Approved, Active, Rejected, Cancelled, ChangesRequired
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


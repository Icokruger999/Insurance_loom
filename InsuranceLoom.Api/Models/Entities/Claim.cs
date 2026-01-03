namespace InsuranceLoom.Api.Models.Entities;

public class Claim
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public Guid? BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public string? ClaimType { get; set; }
    public decimal? ClaimAmount { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Processing
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


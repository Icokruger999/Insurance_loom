namespace InsuranceLoom.Api.Models.Entities;

public class DebitOrder
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BranchCode { get; set; }
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty; // Monthly, Quarterly, Annually
    public DateTime? NextDebitDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Suspended, Cancelled
    public DateTime? LastDebitDate { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


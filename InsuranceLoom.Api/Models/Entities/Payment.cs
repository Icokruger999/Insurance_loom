namespace InsuranceLoom.Api.Models.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public string PaymentType { get; set; } = string.Empty; // Premium, Claim, Refund
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public string? TransactionReference { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


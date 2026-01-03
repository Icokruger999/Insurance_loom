namespace InsuranceLoom.Api.Models.Entities;

public class Beneficiary
{
    public Guid Id { get; set; }
    public Guid PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public Guid? PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Relationship { get; set; }
    public string? Type { get; set; } // Revocable, Irrevocable
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


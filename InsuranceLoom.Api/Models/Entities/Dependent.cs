namespace InsuranceLoom.Api.Models.Entities;

public class Dependent
{
    public Guid Id { get; set; }
    public Guid PolicyHolderId { get; set; } // Link to the primary/main policy holder
    public PolicyHolder? PolicyHolder { get; set; }
    public Guid? PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Relationship { get; set; } // Spouse, Child, Parent, etc.
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


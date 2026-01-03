namespace InsuranceLoom.Api.Models.Entities;

public class Broker
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string AgentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public decimal? CommissionRate { get; set; }
    public bool IsActive { get; set; } = false; // Set to false until approved by admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


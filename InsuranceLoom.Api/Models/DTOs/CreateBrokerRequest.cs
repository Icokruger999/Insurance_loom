namespace InsuranceLoom.Api.Models.DTOs;

public class CreateBrokerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // AgentNumber is auto-generated, not provided by user
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string ManagerEmail { get; set; } = string.Empty; // Manager email for approval
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    // CommissionRate uses default value, can be changed later
}

public class BrokerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string AgentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public decimal? CommissionRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}


namespace InsuranceLoom.Api.Models.DTOs;

public class CreateBrokerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AgentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public decimal? CommissionRate { get; set; }
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


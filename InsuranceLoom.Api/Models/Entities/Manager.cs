namespace InsuranceLoom.Api.Models.Entities;

public class Manager
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CanApprovePolicies { get; set; } = true;
    public bool CanManageBrokers { get; set; } = false;
    public bool CanViewReports { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


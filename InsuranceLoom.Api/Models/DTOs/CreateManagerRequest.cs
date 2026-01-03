namespace InsuranceLoom.Api.Models.DTOs;

public class CreateManagerRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public string Password { get; set; } = string.Empty;
    public ManagerPermissionsDto Permissions { get; set; } = new();
}

public class ManagerPermissionsDto
{
    public bool CanApprovePolicies { get; set; } = true;
    public bool CanManageBrokers { get; set; } = false;
    public bool CanViewReports { get; set; } = true;
}

public class ManagerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public ManagerPermissionsDto Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}


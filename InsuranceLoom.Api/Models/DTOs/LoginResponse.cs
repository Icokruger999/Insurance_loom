namespace InsuranceLoom.Api.Models.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public BrokerInfo? Broker { get; set; }
    public PolicyHolderInfo? PolicyHolder { get; set; }
    public ManagerInfo? Manager { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
}

public class BrokerInfo
{
    public Guid Id { get; set; }
    public string AgentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class PolicyHolderInfo
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ManagerInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public ManagerPermissions? Permissions { get; set; }
}

public class ManagerPermissions
{
    public bool CanApprovePolicies { get; set; }
    public bool CanManageBrokers { get; set; }
    public bool CanViewReports { get; set; }
}


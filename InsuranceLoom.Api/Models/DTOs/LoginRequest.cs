namespace InsuranceLoom.Api.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class BrokerLoginRequest
{
    public string AgentNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PolicyHolderLoginRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ManagerLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


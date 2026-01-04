namespace InsuranceLoom.Api.Models.DTOs;

public class SubmitPolicyForApprovalRequest
{
    public string ManagerEmail { get; set; } = string.Empty;
    public string? Notes { get; set; }
}


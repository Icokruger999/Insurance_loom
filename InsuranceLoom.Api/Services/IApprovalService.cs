using InsuranceLoom.Api.Models.DTOs;

namespace InsuranceLoom.Api.Services;

public interface IApprovalService
{
    Task<PolicyApprovalDto> SubmitPolicyForApprovalAsync(SubmitPolicyRequest request, Guid brokerId);
    Task<List<PolicyApprovalDto>> GetPendingApprovalsAsync(Guid? managerId);
    Task<PolicyApprovalDto?> GetApprovalDetailsAsync(Guid approvalId);
    Task<PolicyApprovalDto?> GetApprovalByPolicyIdAsync(Guid policyId);
    Task<bool> ApprovePolicyAsync(ApprovePolicyRequest request, Guid managerId);
    Task<bool> RejectPolicyAsync(RejectPolicyRequest request, Guid managerId);
    Task<bool> RequestChangesAsync(RequestChangesRequest request, Guid managerId);
    Task<bool> AssignManagerAsync(AssignManagerRequest request);
    Task<ApprovalStatisticsDto> GetApprovalStatisticsAsync(Guid? managerId);
}


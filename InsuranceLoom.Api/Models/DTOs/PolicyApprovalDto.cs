namespace InsuranceLoom.Api.Models.DTOs;

public class SubmitPolicyRequest
{
    public Guid PolicyId { get; set; }
    public string? Notes { get; set; }
}

public class PolicyApprovalDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public Guid? AssignedManagerId { get; set; }
    public string? AssignedManagerName { get; set; }
    public DateTime? AssignedDate { get; set; }
    public bool DocumentsVerified { get; set; }
    public PolicyInfo? Policy { get; set; }
    public BrokerInfo? Broker { get; set; }
    public PolicyHolderInfo? PolicyHolder { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
}

public class PolicyInfo
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ApprovePolicyRequest
{
    public Guid ApprovalId { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class RejectPolicyRequest
{
    public Guid ApprovalId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class RequestChangesRequest
{
    public Guid ApprovalId { get; set; }
    public string ChangesRequired { get; set; } = string.Empty;
}

public class AssignManagerRequest
{
    public Guid ApprovalId { get; set; }
    public Guid ManagerId { get; set; }
    public string? AssignmentNotes { get; set; }
}

public class ApprovalStatisticsDto
{
    public int PendingCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int ApprovedTodayCount { get; set; }
    public int RejectedTodayCount { get; set; }
    public double AverageReviewTimeHours { get; set; }
    public double ApprovalRate { get; set; }
}


using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Services;

public class ApprovalService : IApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentService _documentService;
    private readonly IEmailService _emailService;

    public ApprovalService(ApplicationDbContext context, IDocumentService documentService, IEmailService emailService)
    {
        _context = context;
        _documentService = documentService;
        _emailService = emailService;
    }

    public async Task<PolicyApprovalDto> SubmitPolicyForApprovalAsync(SubmitPolicyRequest request, Guid brokerId)
    {
        var policy = await _context.Policies
            .Include(p => p.PolicyHolder)
            .Include(p => p.Broker)
            .Include(p => p.ServiceType)
            .FirstOrDefaultAsync(p => p.Id == request.PolicyId && p.BrokerId == brokerId);

        if (policy == null)
            throw new ArgumentException("Policy not found");

        if (policy.Status != "Draft" && policy.Status != "PendingSubmission" && policy.Status != "ChangesRequired")
            throw new InvalidOperationException($"Policy cannot be submitted. Current status: {policy.Status}");

        // Check if all required documents are uploaded and verified
        var requiredDocs = await _documentService.GetRequiredDocumentsAsync(policy.ServiceType.ServiceCode, policy.PolicyHolderId);
        var missingDocs = requiredDocs.RequiredDocuments.Where(d => !d.IsUploaded || d.Status != "Verified").ToList();

        if (missingDocs.Any())
            throw new InvalidOperationException($"Missing or unverified required documents: {string.Join(", ", missingDocs.Select(d => d.DocumentName))}");

        // Create or update approval record
        var approval = await _context.PolicyApprovals
            .FirstOrDefaultAsync(a => a.PolicyId == request.PolicyId);

        if (approval == null)
        {
            approval = new PolicyApproval
            {
                Id = Guid.NewGuid(),
                PolicyId = request.PolicyId,
                BrokerId = brokerId,
                PolicyHolderId = policy.PolicyHolderId,
                Status = "Pending",
                SubmittedBy = brokerId,
                SubmittedDate = DateTime.UtcNow
            };
            _context.PolicyApprovals.Add(approval);
        }
        else
        {
            approval.Status = "Pending";
            approval.SubmittedBy = brokerId;
            approval.SubmittedDate = DateTime.UtcNow;
            approval.UpdatedAt = DateTime.UtcNow;
        }

        // Update policy status
        policy.Status = "Submitted";
        policy.UpdatedAt = DateTime.UtcNow;

        // Auto-assign to manager (round-robin or based on rules)
        var availableManager = await GetAvailableManagerAsync();
        if (availableManager != null)
        {
            approval.AssignedManagerId = availableManager.Id;
            approval.AssignedDate = DateTime.UtcNow;
            approval.Status = "UnderReview";

            // Send notification
            await _emailService.SendPolicySubmittedNotificationAsync(
                availableManager.Email,
                policy.PolicyNumber,
                $"{policy.Broker.FirstName} {policy.Broker.LastName}"
            );
        }

        await _context.SaveChangesAsync();

        // Add history
        await AddApprovalHistory(approval.Id, policy.Id, "Submitted", brokerId, "Broker", policy.Status, "Submitted", request.Notes);

        return await GetApprovalDetailsAsync(approval.Id) ?? throw new Exception("Failed to create approval");
    }

    public async Task<List<PolicyApprovalDto>> GetPendingApprovalsAsync(Guid? managerId)
    {
        var query = _context.PolicyApprovals
            .Include(a => a.Policy)
                .ThenInclude(p => p.ServiceType)
            .Include(a => a.Broker)
            .Include(a => a.PolicyHolder)
            .Include(a => a.AssignedManager)
            .AsQueryable();

        if (managerId.HasValue)
        {
            query = query.Where(a => a.AssignedManagerId == managerId && 
                (a.Status == "Pending" || a.Status == "UnderReview"));
        }
        else
        {
            query = query.Where(a => a.Status == "Pending" || a.Status == "UnderReview");
        }

        var approvals = await query
            .OrderByDescending(a => a.SubmittedDate)
            .ToListAsync();

        var result = new List<PolicyApprovalDto>();

        foreach (var approval in approvals)
        {
            var documents = await _documentService.GetDocumentsAsync(approval.PolicyHolderId, approval.PolicyId);

            result.Add(new PolicyApprovalDto
            {
                Id = approval.Id,
                PolicyId = approval.PolicyId,
                PolicyNumber = approval.Policy.PolicyNumber,
                Status = approval.Status,
                SubmittedDate = approval.SubmittedDate,
                AssignedManagerId = approval.AssignedManagerId,
                AssignedManagerName = approval.AssignedManager != null 
                    ? $"{approval.AssignedManager.FirstName} {approval.AssignedManager.LastName}" 
                    : null,
                AssignedDate = approval.AssignedDate,
                DocumentsVerified = approval.DocumentsVerified,
                Policy = new PolicyInfo
                {
                    Id = approval.Policy.Id,
                    PolicyNumber = approval.Policy.PolicyNumber,
                    ServiceType = approval.Policy.ServiceType.ServiceName,
                    CoverageAmount = approval.Policy.CoverageAmount,
                    PremiumAmount = approval.Policy.PremiumAmount,
                    StartDate = approval.Policy.StartDate,
                    EndDate = approval.Policy.EndDate,
                    Status = approval.Policy.Status
                },
                Broker = new BrokerInfo
                {
                    Id = approval.Broker.Id,
                    AgentNumber = approval.Broker.AgentNumber,
                    FirstName = approval.Broker.FirstName,
                    LastName = approval.Broker.LastName,
                    CompanyName = approval.Broker.CompanyName
                },
                PolicyHolder = new PolicyHolderInfo
                {
                    Id = approval.PolicyHolder.Id,
                    PolicyNumber = approval.PolicyHolder.PolicyNumber,
                    FirstName = approval.PolicyHolder.FirstName,
                    LastName = approval.PolicyHolder.LastName
                },
                Documents = documents
            });
        }

        return result;
    }

    public async Task<PolicyApprovalDto?> GetApprovalDetailsAsync(Guid approvalId)
    {
        var approval = await _context.PolicyApprovals
            .Include(a => a.Policy)
                .ThenInclude(p => p.ServiceType)
            .Include(a => a.Broker)
            .Include(a => a.PolicyHolder)
            .Include(a => a.AssignedManager)
            .FirstOrDefaultAsync(a => a.Id == approvalId);

        if (approval == null)
            return null;

        var documents = await _documentService.GetDocumentsAsync(approval.PolicyHolderId, approval.PolicyId);

        return new PolicyApprovalDto
        {
            Id = approval.Id,
            PolicyId = approval.PolicyId,
            PolicyNumber = approval.Policy.PolicyNumber,
            Status = approval.Status,
            SubmittedDate = approval.SubmittedDate,
            AssignedManagerId = approval.AssignedManagerId,
            AssignedManagerName = approval.AssignedManager != null 
                ? $"{approval.AssignedManager.FirstName} {approval.AssignedManager.LastName}" 
                : null,
            AssignedDate = approval.AssignedDate,
            DocumentsVerified = approval.DocumentsVerified,
            Policy = new PolicyInfo
            {
                Id = approval.Policy.Id,
                PolicyNumber = approval.Policy.PolicyNumber,
                ServiceType = approval.Policy.ServiceType.ServiceName,
                CoverageAmount = approval.Policy.CoverageAmount,
                PremiumAmount = approval.Policy.PremiumAmount,
                StartDate = approval.Policy.StartDate,
                EndDate = approval.Policy.EndDate,
                Status = approval.Policy.Status
            },
            Broker = new BrokerInfo
            {
                Id = approval.Broker.Id,
                AgentNumber = approval.Broker.AgentNumber,
                FirstName = approval.Broker.FirstName,
                LastName = approval.Broker.LastName,
                CompanyName = approval.Broker.CompanyName
            },
            PolicyHolder = new PolicyHolderInfo
            {
                Id = approval.PolicyHolder.Id,
                PolicyNumber = approval.PolicyHolder.PolicyNumber,
                FirstName = approval.PolicyHolder.FirstName,
                LastName = approval.PolicyHolder.LastName
            },
            Documents = documents
        };
    }

    public async Task<PolicyApprovalDto?> GetApprovalByPolicyIdAsync(Guid policyId)
    {
        var approval = await _context.PolicyApprovals
            .Include(a => a.Policy)
                .ThenInclude(p => p.ServiceType)
            .Include(a => a.Broker)
            .Include(a => a.PolicyHolder)
            .Include(a => a.AssignedManager)
            .FirstOrDefaultAsync(a => a.PolicyId == policyId);

        if (approval == null)
            return null;

        var documents = await _documentService.GetDocumentsAsync(approval.PolicyHolderId, approval.PolicyId);

        return new PolicyApprovalDto
        {
            Id = approval.Id,
            PolicyId = approval.PolicyId,
            PolicyNumber = approval.Policy.PolicyNumber,
            Status = approval.Status,
            SubmittedDate = approval.SubmittedDate,
            AssignedManagerId = approval.AssignedManagerId,
            AssignedManagerName = approval.AssignedManager != null 
                ? $"{approval.AssignedManager.FirstName} {approval.AssignedManager.LastName}" 
                : null,
            AssignedDate = approval.AssignedDate,
            DocumentsVerified = approval.DocumentsVerified,
            Policy = new PolicyInfo
            {
                Id = approval.Policy.Id,
                PolicyNumber = approval.Policy.PolicyNumber,
                ServiceType = approval.Policy.ServiceType.ServiceName,
                CoverageAmount = approval.Policy.CoverageAmount,
                PremiumAmount = approval.Policy.PremiumAmount,
                StartDate = approval.Policy.StartDate,
                EndDate = approval.Policy.EndDate,
                Status = approval.Policy.Status
            },
            Broker = new BrokerInfo
            {
                Id = approval.Broker.Id,
                AgentNumber = approval.Broker.AgentNumber,
                FirstName = approval.Broker.FirstName,
                LastName = approval.Broker.LastName,
                CompanyName = approval.Broker.CompanyName
            },
            PolicyHolder = new PolicyHolderInfo
            {
                Id = approval.PolicyHolder.Id,
                PolicyNumber = approval.PolicyHolder.PolicyNumber,
                FirstName = approval.PolicyHolder.FirstName,
                LastName = approval.PolicyHolder.LastName
            },
            Documents = documents
        };
    }

    public async Task<bool> ApprovePolicyAsync(ApprovePolicyRequest request, Guid managerId)
    {
        var approval = await _context.PolicyApprovals
            .Include(a => a.Policy)
            .Include(a => a.Broker)
            .Include(a => a.PolicyHolder)
            .FirstOrDefaultAsync(a => a.Id == request.ApprovalId);

        if (approval == null)
            return false;

        if (approval.Status != "UnderReview" && approval.Status != "Pending")
            throw new InvalidOperationException($"Cannot approve policy in status: {approval.Status}");

        // Verify manager has permission
        var manager = await _context.Managers.FindAsync(managerId);
        if (manager == null || !manager.CanApprovePolicies)
            throw new UnauthorizedAccessException("Manager does not have approval permissions");

        // Update approval
        approval.Status = "Approved";
        approval.ApprovedBy = managerId;
        approval.ApprovedDate = DateTime.UtcNow;
        approval.ApprovalNotes = request.ApprovalNotes;
        approval.UpdatedAt = DateTime.UtcNow;

        // Update policy
        approval.Policy.Status = "Active";
        approval.Policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Add history
        await AddApprovalHistory(approval.Id, approval.PolicyId, "Approved", managerId, "Manager", 
            approval.Status, "Approved", request.ApprovalNotes);

        // Send notifications
        var brokerEmail = approval.Broker.User?.Email ?? string.Empty;
        var policyHolderEmail = approval.PolicyHolder.User?.Email ?? string.Empty;
        await _emailService.SendPolicyApprovedNotificationAsync(
            brokerEmail,
            policyHolderEmail,
            approval.Policy.PolicyNumber
        );

        return true;
    }

    public async Task<bool> RejectPolicyAsync(RejectPolicyRequest request, Guid managerId)
    {
        var approval = await _context.PolicyApprovals
            .Include(a => a.Policy)
            .Include(a => a.Broker)
            .FirstOrDefaultAsync(a => a.Id == request.ApprovalId);

        if (approval == null)
            return false;

        var manager = await _context.Managers.FindAsync(managerId);
        if (manager == null || !manager.CanApprovePolicies)
            throw new UnauthorizedAccessException("Manager does not have approval permissions");

        approval.Status = "Rejected";
        approval.RejectedBy = managerId;
        approval.RejectedDate = DateTime.UtcNow;
        approval.RejectionReason = request.RejectionReason;
        approval.UpdatedAt = DateTime.UtcNow;

        approval.Policy.Status = "Rejected";
        approval.Policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddApprovalHistory(approval.Id, approval.PolicyId, "Rejected", managerId, "Manager", 
            "UnderReview", "Rejected", request.RejectionReason);

        var brokerEmail = approval.Broker.User?.Email ?? string.Empty;
        await _emailService.SendPolicyRejectedNotificationAsync(
            brokerEmail,
            approval.Policy.PolicyNumber,
            request.RejectionReason
        );

        return true;
    }

    public async Task<bool> RequestChangesAsync(RequestChangesRequest request, Guid managerId)
    {
        var approval = await _context.PolicyApprovals
            .Include(a => a.Policy)
            .Include(a => a.Broker)
            .FirstOrDefaultAsync(a => a.Id == request.ApprovalId);

        if (approval == null)
            return false;

        var manager = await _context.Managers.FindAsync(managerId);
        if (manager == null || !manager.CanApprovePolicies)
            throw new UnauthorizedAccessException("Manager does not have approval permissions");

        approval.Status = "RequiresChanges";
        approval.ChangesRequestedBy = managerId;
        approval.ChangesRequestedDate = DateTime.UtcNow;
        approval.ChangesRequired = request.ChangesRequired;
        approval.UpdatedAt = DateTime.UtcNow;

        approval.Policy.Status = "ChangesRequired";
        approval.Policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddApprovalHistory(approval.Id, approval.PolicyId, "ChangesRequested", managerId, "Manager", 
            "UnderReview", "RequiresChanges", request.ChangesRequired);

        var brokerEmail = approval.Broker.User?.Email ?? string.Empty;
        await _emailService.SendChangesRequestedNotificationAsync(
            brokerEmail,
            approval.Policy.PolicyNumber,
            request.ChangesRequired
        );

        return true;
    }

    public async Task<bool> AssignManagerAsync(AssignManagerRequest request)
    {
        var approval = await _context.PolicyApprovals.FindAsync(request.ApprovalId);
        if (approval == null)
            return false;

        var manager = await _context.Managers.FindAsync(request.ManagerId);
        if (manager == null || !manager.IsActive || !manager.CanApprovePolicies)
            throw new ArgumentException("Invalid or inactive manager");

        approval.AssignedManagerId = request.ManagerId;
        approval.AssignedDate = DateTime.UtcNow;
        approval.Status = "UnderReview";
        approval.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddApprovalHistory(approval.Id, approval.PolicyId, "Assigned", request.ManagerId, "Manager", 
            "Pending", "UnderReview", request.AssignmentNotes);

        return true;
    }

    public async Task<ApprovalStatisticsDto> GetApprovalStatisticsAsync(Guid? managerId)
    {
        var query = _context.PolicyApprovals.AsQueryable();

        if (managerId.HasValue)
            query = query.Where(a => a.AssignedManagerId == managerId);

        var pending = await query.CountAsync(a => a.Status == "Pending");
        var underReview = await query.CountAsync(a => a.Status == "UnderReview");
        var approvedToday = await query.CountAsync(a => 
            a.Status == "Approved" && a.ApprovedDate.HasValue && 
            a.ApprovedDate.Value.Date == DateTime.UtcNow.Date);
        var rejectedToday = await query.CountAsync(a => 
            a.Status == "Rejected" && a.RejectedDate.HasValue && 
            a.RejectedDate.Value.Date == DateTime.UtcNow.Date);

        var completedApprovals = await query
            .Where(a => a.Status == "Approved" && a.ApprovedDate.HasValue)
            .ToListAsync();

        var avgReviewTime = completedApprovals.Any()
            ? completedApprovals.Average(a => (a.ApprovedDate!.Value - a.SubmittedDate).TotalHours)
            : 0;

        var totalProcessed = await query.CountAsync(a => a.Status == "Approved" || a.Status == "Rejected");
        var totalApproved = await query.CountAsync(a => a.Status == "Approved");
        var approvalRate = totalProcessed > 0 ? (double)totalApproved / totalProcessed * 100 : 0;

        return new ApprovalStatisticsDto
        {
            PendingCount = pending,
            UnderReviewCount = underReview,
            ApprovedTodayCount = approvedToday,
            RejectedTodayCount = rejectedToday,
            AverageReviewTimeHours = avgReviewTime,
            ApprovalRate = approvalRate
        };
    }

    private async Task<Manager?> GetAvailableManagerAsync()
    {
        // Simple round-robin: Get manager with least pending approvals
        var managers = await _context.Managers
            .Where(m => m.IsActive && m.CanApprovePolicies)
            .ToListAsync();

        if (!managers.Any())
            return null;

        var managerWorkloads = new List<(Manager Manager, int Count)>();

        foreach (var manager in managers)
        {
            var count = await _context.PolicyApprovals
                .CountAsync(a => a.AssignedManagerId == manager.Id && 
                    (a.Status == "Pending" || a.Status == "UnderReview"));
            managerWorkloads.Add((manager, count));
        }

        return managerWorkloads.OrderBy(m => m.Count).FirstOrDefault().Manager;
    }

    private async Task AddApprovalHistory(Guid approvalId, Guid policyId, string action, Guid? performedBy, 
        string? performedByType, string? previousStatus, string? newStatus, string? notes)
    {
        var history = new PolicyApprovalHistory
        {
            Id = Guid.NewGuid(),
            ApprovalId = approvalId,
            PolicyId = policyId,
            Action = action,
            PerformedBy = performedBy,
            PerformedByType = performedByType,
            Notes = notes,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            CreatedAt = DateTime.UtcNow
        };

        _context.PolicyApprovalHistory.Add(history);
        await _context.SaveChangesAsync();
    }
}


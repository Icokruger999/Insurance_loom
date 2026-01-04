using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/policy-approval")]
[Authorize]
public class PolicyApprovalController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public PolicyApprovalController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("{policyId}/submit")]
    [Authorize(Roles = "Broker")]
    public async Task<IActionResult> SubmitForApproval(Guid policyId, [FromBody] SubmitPolicyForApprovalRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ManagerEmail))
            {
                return BadRequest(new { message = "Manager email is required" });
            }

            var policy = await _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.Broker)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
            {
                return NotFound(new { message = "Policy not found" });
            }

            // Check if policy is already submitted
            if (policy.Status != "Draft" && policy.Status != "ChangesRequired")
            {
                return BadRequest(new { message = $"Policy is already {policy.Status}. Cannot resubmit." });
            }

            // Find manager by email
            var manager = await _context.Managers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.User != null && m.User.Email == request.ManagerEmail);

            if (manager == null)
            {
                return BadRequest(new { message = "Manager not found with the provided email address" });
            }

            // Update policy status
            policy.Status = "PendingSubmission";
            policy.UpdatedAt = DateTime.UtcNow;

            // Get broker ID from authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var brokerUserId))
            {
                return Unauthorized(new { message = "Broker user ID not found in token." });
            }

            var broker = await _context.Brokers
                .FirstOrDefaultAsync(b => b.UserId == brokerUserId);

            // Create or update policy approval record
            var approval = await _context.PolicyApprovals
                .FirstOrDefaultAsync(a => a.PolicyId == policyId);

            if (approval == null)
            {
                approval = new PolicyApproval
                {
                    Id = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    BrokerId = broker?.Id ?? policy.BrokerId,
                    PolicyHolderId = policy.PolicyHolderId,
                    Status = "Pending",
                    SubmittedBy = brokerUserId,
                    SubmittedDate = DateTime.UtcNow,
                    AssignedManagerId = manager.Id,
                    AssignedDate = DateTime.UtcNow,
                    ReviewNotes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PolicyApprovals.Add(approval);
            }
            else
            {
                approval.Status = "Pending";
                approval.SubmittedDate = DateTime.UtcNow;
                approval.AssignedManagerId = manager.Id;
                approval.AssignedDate = DateTime.UtcNow;
                approval.ReviewNotes = request.Notes;
                approval.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Send email notification to manager
            if (manager.User != null)
            {
                try
                {
                    var emailBody = $@"
                        <h2>New Policy Submitted for Approval</h2>
                        <p>Hello {manager.FirstName} {manager.LastName},</p>
                        <p>A new policy has been submitted for your approval:</p>
                        <ul>
                            <li><strong>Policy Number:</strong> {policy.PolicyNumber}</li>
                            <li><strong>Policy Holder:</strong> {policy.PolicyHolder?.FirstName} {policy.PolicyHolder?.LastName}</li>
                            <li><strong>Agent:</strong> {broker?.FirstName} {broker?.LastName}</li>
                            <li><strong>Submitted Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm}</li>
                        </ul>
                        <p>Please log in to your manager portal to review and approve this policy.</p>
                        <p>Best regards,<br>Insurance Loom Team</p>
                    ";

                    await _emailService.SendEmailAsync(
                        manager.User.Email,
                        "New Policy Submitted for Approval",
                        emailBody
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send approval request email: {ex.Message}");
                }
            }

            return Ok(new { message = "Policy submitted for approval successfully", policyId = policy.Id, status = policy.Status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error submitting policy for approval", error = ex.Message });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetPendingApplications()
    {
        try
        {
            // Get manager ID from authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var managerUserId))
            {
                return Unauthorized(new { message = "Manager user ID not found in token." });
            }

            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.UserId == managerUserId);

            if (manager == null)
            {
                return NotFound(new { message = "Manager not found" });
            }

            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.Broker)
                .Where(a => (a.Status == "Pending" || a.Status == "UnderReview") && a.AssignedManagerId == manager.Id)
                .OrderByDescending(a => a.SubmittedDate)
                .Select(a => new
                {
                    id = a.Id,
                    policyId = a.PolicyId,
                    policyNumber = a.Policy!.PolicyNumber,
                    policyHolderName = $"{a.PolicyHolder!.FirstName} {a.PolicyHolder.LastName}",
                    brokerName = $"{a.Broker!.FirstName} {a.Broker.LastName}",
                    serviceType = a.Policy.ServiceType!.ServiceName,
                    coverageAmount = a.Policy.CoverageAmount,
                    premiumAmount = a.Policy.PremiumAmount,
                    status = a.Status,
                    submittedDate = a.SubmittedDate,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching pending applications", error = ex.Message });
        }
    }

    [HttpGet("broker/pending")]
    [Authorize(Roles = "Broker")]
    public async Task<IActionResult> GetBrokerPendingApplications()
    {
        try
        {
            // Get broker ID from authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var brokerUserId))
            {
                return Unauthorized(new { message = "Broker user ID not found in token." });
            }

            var broker = await _context.Brokers
                .FirstOrDefaultAsync(b => b.UserId == brokerUserId);

            if (broker == null)
            {
                return NotFound(new { message = "Broker not found" });
            }

            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.AssignedManager)
                    .ThenInclude(m => m!.User)
                .Where(a => a.BrokerId == broker.Id && (a.Status == "Pending" || a.Status == "UnderReview"))
                .OrderByDescending(a => a.SubmittedDate)
                .Select(a => new
                {
                    id = a.Id,
                    policyId = a.PolicyId,
                    policyNumber = a.Policy!.PolicyNumber,
                    policyHolderName = $"{a.PolicyHolder!.FirstName} {a.PolicyHolder.LastName}",
                    managerName = a.AssignedManager != null ? $"{a.AssignedManager.FirstName} {a.AssignedManager.LastName}" : null,
                    managerEmail = a.AssignedManager != null ? a.AssignedManager.User!.Email : null,
                    serviceType = a.Policy.ServiceType!.ServiceName,
                    coverageAmount = a.Policy.CoverageAmount,
                    premiumAmount = a.Policy.PremiumAmount,
                    status = a.Status,
                    submittedDate = a.SubmittedDate,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching broker pending applications", error = ex.Message });
        }
    }

    [HttpGet("broker/approved")]
    [Authorize(Roles = "Broker")]
    public async Task<IActionResult> GetBrokerApprovedApplications()
    {
        try
        {
            // Get broker ID from authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var brokerUserId))
            {
                return Unauthorized(new { message = "Broker user ID not found in token." });
            }

            var broker = await _context.Brokers
                .FirstOrDefaultAsync(b => b.UserId == brokerUserId);

            if (broker == null)
            {
                return NotFound(new { message = "Broker not found" });
            }

            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.AssignedManager)
                    .ThenInclude(m => m!.User)
                .Where(a => a.BrokerId == broker.Id && a.Status == "Approved")
                .OrderByDescending(a => a.ApprovedDate)
                .Select(a => new
                {
                    id = a.Id,
                    policyId = a.PolicyId,
                    policyNumber = a.Policy!.PolicyNumber,
                    policyHolderName = $"{a.PolicyHolder!.FirstName} {a.PolicyHolder.LastName}",
                    managerName = a.AssignedManager != null ? $"{a.AssignedManager.FirstName} {a.AssignedManager.LastName}" : null,
                    serviceType = a.Policy.ServiceType!.ServiceName,
                    coverageAmount = a.Policy.CoverageAmount,
                    premiumAmount = a.Policy.PremiumAmount,
                    status = a.Status,
                    approvedDate = a.ApprovedDate,
                    documentsVerified = a.DocumentsVerified,
                    submittedDate = a.SubmittedDate
                })
                .ToListAsync();

            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching broker approved applications", error = ex.Message });
        }
    }

    [HttpGet("approved")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetApprovedApplications()
    {
        try
        {
            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.Broker)
                .Where(a => a.Status == "Approved")
                .OrderByDescending(a => a.ApprovedDate)
                .Select(a => new
                {
                    id = a.Id,
                    policyId = a.PolicyId,
                    policyNumber = a.Policy!.PolicyNumber,
                    policyHolderName = $"{a.PolicyHolder!.FirstName} {a.PolicyHolder.LastName}",
                    brokerName = $"{a.Broker!.FirstName} {a.Broker.LastName}",
                    serviceType = a.Policy.ServiceType!.ServiceName,
                    coverageAmount = a.Policy.CoverageAmount,
                    premiumAmount = a.Policy.PremiumAmount,
                    status = a.Status,
                    approvedDate = a.ApprovedDate,
                    approvedBy = a.ApprovedBy,
                    submittedDate = a.SubmittedDate
                })
                .ToListAsync();

            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching approved applications", error = ex.Message });
        }
    }

    [HttpGet("rejected")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetRejectedApplications()
    {
        try
        {
            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.Broker)
                .Where(a => a.Status == "Rejected")
                .OrderByDescending(a => a.RejectedDate)
                .Select(a => new
                {
                    id = a.Id,
                    policyId = a.PolicyId,
                    policyNumber = a.Policy!.PolicyNumber,
                    policyHolderName = $"{a.PolicyHolder!.FirstName} {a.PolicyHolder.LastName}",
                    brokerName = $"{a.Broker!.FirstName} {a.Broker.LastName}",
                    serviceType = a.Policy.ServiceType!.ServiceName,
                    coverageAmount = a.Policy.CoverageAmount,
                    premiumAmount = a.Policy.PremiumAmount,
                    status = a.Status,
                    rejectedDate = a.RejectedDate,
                    rejectionReason = a.RejectionReason,
                    submittedDate = a.SubmittedDate
                })
                .ToListAsync();

            return Ok(applications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching rejected applications", error = ex.Message });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetAllPolicies([FromQuery] string? region = null, [FromQuery] Guid? brokerId = null, [FromQuery] string? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.Broker)
                .Include(p => p.ServiceType)
                .AsQueryable();

            // Filter by region (based on policy holder's province/city)
            if (!string.IsNullOrWhiteSpace(region))
            {
                var regionLower = region.ToLower();
                query = query.Where(p => 
                    p.PolicyHolder != null && 
                    (p.PolicyHolder.Province != null && p.PolicyHolder.Province.ToLower().Contains(regionLower) ||
                     p.PolicyHolder.City != null && p.PolicyHolder.City.ToLower().Contains(regionLower)));
            }

            // Filter by broker
            if (brokerId.HasValue)
            {
                query = query.Where(p => p.BrokerId == brokerId.Value);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Filter by date range (created_at)
            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value.AddDays(1)); // Include the entire end date
            }

            var policies = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    id = p.Id,
                    policyNumber = p.PolicyNumber,
                    policyHolderName = p.PolicyHolder != null ? $"{p.PolicyHolder.FirstName} {p.PolicyHolder.LastName}" : "N/A",
                    policyHolderCity = p.PolicyHolder != null ? p.PolicyHolder.City : null,
                    policyHolderProvince = p.PolicyHolder != null ? p.PolicyHolder.Province : null,
                    brokerId = p.BrokerId,
                    brokerName = p.Broker != null ? $"{p.Broker.FirstName} {p.Broker.LastName}" : "N/A",
                    serviceType = p.ServiceType != null ? p.ServiceType.ServiceName : "N/A",
                    coverageAmount = p.CoverageAmount,
                    premiumAmount = p.PremiumAmount,
                    status = p.Status,
                    startDate = p.StartDate,
                    createdAt = p.CreatedAt,
                    updatedAt = p.UpdatedAt
                })
                .ToListAsync();

            // Get policy IDs for approval lookup
            var policyIds = policies.Select(p => p.id).ToList();

            // Get approval statuses separately
            var approvals = await _context.PolicyApprovals
                .Where(pa => policyIds.Contains(pa.PolicyId))
                .GroupBy(pa => pa.PolicyId)
                .Select(g => new
                {
                    PolicyId = g.Key,
                    ApprovalStatus = g.OrderByDescending(pa => pa.SubmittedDate).Select(pa => pa.Status).FirstOrDefault(),
                    DocumentsVerified = g.OrderByDescending(pa => pa.SubmittedDate).Select(pa => pa.DocumentsVerified).FirstOrDefault()
                })
                .ToListAsync();

            // Get document counts separately
            var documentCounts = await _context.Documents
                .Where(d => d.PolicyId.HasValue && policyIds.Contains(d.PolicyId.Value))
                .GroupBy(d => d.PolicyId!.Value)
                .Select(g => new { PolicyId = g.Key, HasDocuments = g.Any() })
                .ToListAsync();
            
            // Convert to dictionary for faster lookup
            var documentDict = documentCounts.ToDictionary(d => d.PolicyId, d => d.HasDocuments);

            // Merge approval data
            var result = policies.Select(p => new
            {
                p.id,
                p.policyNumber,
                p.policyHolderName,
                p.policyHolderCity,
                p.policyHolderProvince,
                p.brokerId,
                p.brokerName,
                p.serviceType,
                p.coverageAmount,
                p.premiumAmount,
                p.status,
                p.startDate,
                p.createdAt,
                p.updatedAt,
                approvalStatus = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.ApprovalStatus,
                hasDocuments = documentDict.ContainsKey(p.id) && documentDict[p.id],
                documentsVerified = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.DocumentsVerified ?? false
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching policies", error = ex.Message });
        }
    }

    [HttpPost("{policyId}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ApprovePolicy(Guid policyId, [FromBody] ApprovalRequest? request = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var managerUserId))
            {
                return Unauthorized(new { message = "Manager user ID not found in token." });
            }

            var policy = await _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.Broker)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
            {
                return NotFound(new { message = "Policy not found" });
            }

            var approval = await _context.PolicyApprovals
                .FirstOrDefaultAsync(a => a.PolicyId == policyId);

            if (approval == null)
            {
                return NotFound(new { message = "Policy approval record not found" });
            }

            // Update policy status
            policy.Status = "Approved";
            policy.UpdatedAt = DateTime.UtcNow;

            // Update approval record
            approval.Status = "Approved";
            approval.ApprovedBy = managerUserId;
            approval.ApprovedDate = DateTime.UtcNow;
            approval.ApprovalNotes = request?.Notes;
            approval.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send approval notification
            var broker = await _context.Brokers.FirstOrDefaultAsync(b => b.Id == policy.BrokerId);
            if (broker != null)
            {
                var brokerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == broker.UserId);
                var policyHolderUser = policy.PolicyHolder?.UserId != null 
                    ? await _context.Users.FirstOrDefaultAsync(u => u.Id == policy.PolicyHolder.UserId)
                    : null;
                
                if (brokerUser != null)
                {
                    try
                    {
                        await _emailService.SendPolicyApprovedNotificationAsync(
                            brokerUser.Email,
                            policyHolderUser?.Email ?? "",
                            policy.PolicyNumber
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send approval email: {ex.Message}");
                    }
                }
            }

            return Ok(new { message = "Policy approved successfully", policyId = policy.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error approving policy", error = ex.Message });
        }
    }

    [HttpPost("{policyId}/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> RejectPolicy(Guid policyId, [FromBody] RejectionRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var managerUserId))
            {
                return Unauthorized(new { message = "Manager user ID not found in token." });
            }

            var policy = await _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.Broker)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
            {
                return NotFound(new { message = "Policy not found" });
            }

            var approval = await _context.PolicyApprovals
                .FirstOrDefaultAsync(a => a.PolicyId == policyId);

            if (approval == null)
            {
                return NotFound(new { message = "Policy approval record not found" });
            }

            // Update policy status
            policy.Status = "Rejected";
            policy.UpdatedAt = DateTime.UtcNow;

            // Update approval record
            approval.Status = "Rejected";
            approval.RejectedBy = managerUserId;
            approval.RejectedDate = DateTime.UtcNow;
            approval.RejectionReason = request.Reason;
            approval.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send rejection notification
            var broker = await _context.Brokers.FirstOrDefaultAsync(b => b.Id == policy.BrokerId);
            if (broker != null)
            {
                var brokerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == broker.UserId);
                if (brokerUser != null)
                {
                    try
                    {
                        await _emailService.SendPolicyRejectedNotificationAsync(
                            brokerUser.Email,
                            policy.PolicyNumber,
                            request.Reason
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send rejection email: {ex.Message}");
                    }
                }
            }

            return Ok(new { message = "Policy rejected successfully", policyId = policy.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error rejecting policy", error = ex.Message });
        }
    }

    [HttpGet("agents/activity/stats")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetBrokerActivityStats()
    {
        try
        {
            var policies = await _context.Policies
                .ToListAsync();

            var active = policies.Count(p => p.Status == "Active" || p.Status == "Approved");
            var expired = policies.Count(p => p.Status == "Rejected" || p.Status == "Cancelled" || (p.EndDate.HasValue && p.EndDate.Value < DateTime.UtcNow));
            var pending = policies.Count(p => p.Status == "PendingSubmission" || p.Status == "Submitted" || p.Status == "UnderReview" || p.Status == "Draft");

            var approvals = await _context.PolicyApprovals
                .Where(a => a.Status == "Approved" || a.Status == "Pending")
                .ToListAsync();

            var reviewed = approvals.Count(a => a.Status == "Approved");
            var pendingReview = approvals.Count(a => a.Status == "Pending" || a.Status == "UnderReview");

            // Last 7 days renewals (policies created in last 7 days)
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var recentPolicies = policies.Where(p => p.CreatedAt >= sevenDaysAgo).ToList();
            var renewalsReviewed = recentPolicies.Count(p => p.Status == "Approved" || p.Status == "Active");
            var renewalsPending = recentPolicies.Count(p => p.Status == "PendingSubmission" || p.Status == "Submitted" || p.Status == "UnderReview" || p.Status == "Draft");

            return Ok(new
            {
                active,
                expired,
                pending,
                reviewed,
                pendingReview,
                renewalsReviewed,
                renewalsPending
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching broker activity stats", error = ex.Message });
        }
    }

    [HttpGet("agents/activity/latest-policies")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetLatestPoliciesSold()
    {
        try
        {
            var policies = await _context.Policies
                .Include(p => p.Broker)
                .Include(p => p.ServiceType)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new
                {
                    policyId = p.Id,
                    policyNumber = p.PolicyNumber,
                    brokerName = p.Broker != null ? $"{p.Broker.FirstName} {p.Broker.LastName}" : "Unknown",
                    brokerPhone = p.Broker != null ? p.Broker.Phone : null,
                    serviceType = p.ServiceType != null ? p.ServiceType.ServiceName : "N/A",
                    status = p.Status,
                    startDate = p.StartDate,
                    endDate = p.EndDate,
                    coverageAmount = p.CoverageAmount,
                    premiumAmount = p.PremiumAmount,
                    createdAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(policies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching latest policies", error = ex.Message });
        }
    }

    [HttpGet("agents/activity/performance")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetBrokerPerformance()
    {
        try
        {
            // Load data first, then do string operations client-side
            var policies = await _context.Policies
                .Include(p => p.Broker)
                .Where(p => p.Broker != null)
                .ToListAsync();

            var brokerPerformance = policies
                .GroupBy(p => new { p.BrokerId, p.Broker!.FirstName, p.Broker.LastName })
                .Select(g => new
                {
                    brokerId = g.Key.BrokerId,
                    brokerName = $"{g.Key.FirstName} {g.Key.LastName}",
                    policiesCount = g.Count(),
                    totalPremium = g.Sum(p => p.PremiumAmount ?? 0),
                    totalCoverage = g.Sum(p => p.CoverageAmount ?? 0),
                    activePolicies = g.Count(p => p.Status == "Active" || p.Status == "Approved"),
                    date = g.Max(p => p.CreatedAt)
                })
                .OrderByDescending(b => b.totalPremium)
                .Take(10)
                .ToList();

            return Ok(brokerPerformance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching broker performance", error = ex.Message });
        }
    }

    [HttpGet("regions/statistics")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetRegionStatistics()
    {
        try
        {
            var regionStats = await _context.Policies
                .Include(p => p.PolicyHolder)
                .Where(p => p.PolicyHolder != null && (p.PolicyHolder.Province != null || p.PolicyHolder.City != null))
                .GroupBy(p => new
                {
                    Region = p.PolicyHolder!.Province ?? p.PolicyHolder.City ?? "Unknown",
                    City = p.PolicyHolder.City ?? "Unknown"
                })
                .Select(g => new
                {
                    name = g.Key.Region,
                    city = g.Key.City,
                    policies = g.Count(),
                    premium = g.Sum(p => p.PremiumAmount ?? 0),
                    coverage = g.Sum(p => p.CoverageAmount ?? 0),
                    activePolicies = g.Count(p => p.Status == "Active" || p.Status == "Approved"),
                    pendingPolicies = g.Count(p => p.Status == "PendingSubmission" || p.Status == "Submitted" || p.Status == "UnderReview" || p.Status == "Draft")
                })
                .OrderByDescending(r => r.policies)
                .ToListAsync();

            return Ok(regionStats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching region statistics", error = ex.Message });
        }
    }

    [HttpGet("detailed")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetDetailedPolicies(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Policies
                .Include(p => p.PolicyHolder)
                .Include(p => p.Broker)
                .Include(p => p.ServiceType)
                .AsQueryable();

            // Filter by date range (created_at)
            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value.AddDays(1)); // Include the entire end date
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var policies = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    policyNumber = p.PolicyNumber,
                    policyHolderName = p.PolicyHolder != null ? $"{p.PolicyHolder.FirstName} {p.PolicyHolder.LastName}" : "N/A",
                    policyHolderPhone = p.PolicyHolder != null ? p.PolicyHolder.Phone : null,
                    policyHolderCity = p.PolicyHolder != null ? p.PolicyHolder.City : null,
                    policyHolderProvince = p.PolicyHolder != null ? p.PolicyHolder.Province : null,
                    brokerId = p.BrokerId,
                    brokerName = p.Broker != null ? $"{p.Broker.FirstName} {p.Broker.LastName}" : "N/A",
                    serviceType = p.ServiceType != null ? p.ServiceType.ServiceName : "N/A",
                    coverageAmount = p.CoverageAmount,
                    premiumAmount = p.PremiumAmount,
                    status = p.Status,
                    startDate = p.StartDate,
                    endDate = p.EndDate,
                    createdAt = p.CreatedAt,
                    updatedAt = p.UpdatedAt
                })
                .ToListAsync();

            // Get policy IDs for approval lookup
            var policyIds = policies.Select(p => p.id).ToList();

            // Get approval data with rejection reasons
            var approvals = await _context.PolicyApprovals
                .Where(pa => policyIds.Contains(pa.PolicyId))
                .OrderByDescending(pa => pa.SubmittedDate)
                .GroupBy(pa => pa.PolicyId)
                .Select(g => new
                {
                    PolicyId = g.Key,
                    ApprovalStatus = g.First().Status,
                    RejectionReason = g.First().RejectionReason,
                    RejectedDate = g.First().RejectedDate,
                    RejectedBy = g.First().RejectedBy,
                    ApprovalNotes = g.First().ApprovalNotes,
                    ReviewNotes = g.First().ReviewNotes,
                    ChangesRequired = g.First().ChangesRequired
                })
                .ToListAsync();

            // Merge approval data
            var result = policies.Select(p => new
            {
                p.id,
                p.policyNumber,
                p.policyHolderName,
                p.policyHolderPhone,
                p.policyHolderCity,
                p.policyHolderProvince,
                p.brokerId,
                p.brokerName,
                p.serviceType,
                p.coverageAmount,
                p.premiumAmount,
                p.status,
                p.startDate,
                p.endDate,
                p.createdAt,
                p.updatedAt,
                approvalStatus = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.ApprovalStatus,
                rejectionReason = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.RejectionReason,
                rejectedDate = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.RejectedDate,
                approvalNotes = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.ApprovalNotes,
                reviewNotes = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.ReviewNotes,
                changesRequired = approvals.FirstOrDefault(a => a.PolicyId == p.id)?.ChangesRequired
            }).ToList();

            return Ok(new
            {
                policies = result,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching detailed policies", error = ex.Message });
        }
    }
}

public class ApprovalRequest
{
    public string? Notes { get; set; }
}

public class RejectionRequest
{
    public string Reason { get; set; } = string.Empty;
}


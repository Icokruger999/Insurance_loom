using InsuranceLoom.Api.Data;
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
    public async Task<IActionResult> SubmitForApproval(Guid policyId)
    {
        try
        {
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

            // Update policy status
            policy.Status = "PendingSubmission";
            policy.UpdatedAt = DateTime.UtcNow;

            // Create or update policy approval record
            var approval = await _context.PolicyApprovals
                .FirstOrDefaultAsync(a => a.PolicyId == policyId);

            if (approval == null)
            {
                // Get broker ID from authenticated user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var brokerUserId))
                {
                    return Unauthorized(new { message = "Broker user ID not found in token." });
                }

                var broker = await _context.Brokers
                    .FirstOrDefaultAsync(b => b.UserId == brokerUserId);

                approval = new PolicyApproval
                {
                    Id = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    BrokerId = broker?.Id ?? policy.BrokerId,
                    PolicyHolderId = policy.PolicyHolderId,
                    Status = "Pending",
                    SubmittedBy = brokerUserId,
                    SubmittedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PolicyApprovals.Add(approval);
            }
            else
            {
                approval.Status = "Pending";
                approval.SubmittedDate = DateTime.UtcNow;
                approval.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // TODO: Send email notification to managers
            // await _emailService.SendPolicySubmittedNotificationAsync(...);

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
            var applications = await _context.PolicyApprovals
                .Include(a => a.Policy)
                    .ThenInclude(p => p.PolicyHolder)
                .Include(a => a.Policy)
                    .ThenInclude(p => p.ServiceType)
                .Include(a => a.Broker)
                .Where(a => a.Status == "Pending" || a.Status == "UnderReview")
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
                var policyHolderUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == policy.PolicyHolder.UserId);
                
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
}

public class ApprovalRequest
{
    public string? Notes { get; set; }
}

public class RejectionRequest
{
    public string Reason { get; set; } = string.Empty;
}


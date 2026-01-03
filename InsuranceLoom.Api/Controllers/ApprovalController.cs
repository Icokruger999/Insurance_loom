using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpPost("submit")]
    [Authorize(Roles = "Broker")]
    public async Task<ActionResult<PolicyApprovalDto>> SubmitPolicy([FromBody] SubmitPolicyRequest request)
    {
        try
        {
            var brokerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var approval = await _approvalService.SubmitPolicyForApprovalAsync(request, brokerId);
            return Ok(approval);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<List<PolicyApprovalDto>>> GetPendingApprovals()
    {
        try
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guidManagerId = managerId != null ? Guid.Parse(managerId) : (Guid?)null;
            var approvals = await _approvalService.GetPendingApprovalsAsync(guidManagerId);
            return Ok(approvals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Manager,Admin,Broker")]
    public async Task<ActionResult<PolicyApprovalDto>> GetApprovalDetails(Guid id)
    {
        try
        {
            var approval = await _approvalService.GetApprovalDetailsAsync(id);
            if (approval == null)
                return NotFound(new { message = "Approval not found" });

            return Ok(approval);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost("approve")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> ApprovePolicy([FromBody] ApprovePolicyRequest request)
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var result = await _approvalService.ApprovePolicyAsync(request, managerId);
            
            if (!result)
                return NotFound(new { message = "Approval not found" });

            return Ok(new { message = "Policy approved successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost("reject")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> RejectPolicy([FromBody] RejectPolicyRequest request)
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var result = await _approvalService.RejectPolicyAsync(request, managerId);
            
            if (!result)
                return NotFound(new { message = "Approval not found" });

            return Ok(new { message = "Policy rejected" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost("request-changes")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> RequestChanges([FromBody] RequestChangesRequest request)
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var result = await _approvalService.RequestChangesAsync(request, managerId);
            
            if (!result)
                return NotFound(new { message = "Approval not found" });

            return Ok(new { message = "Changes requested" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> AssignManager([FromBody] AssignManagerRequest request)
    {
        try
        {
            var result = await _approvalService.AssignManagerAsync(request);
            
            if (!result)
                return NotFound(new { message = "Approval not found" });

            return Ok(new { message = "Manager assigned successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult<ApprovalStatisticsDto>> GetStatistics()
    {
        try
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guidManagerId = managerId != null ? Guid.Parse(managerId) : (Guid?)null;
            var statistics = await _approvalService.GetApprovalStatisticsAsync(guidManagerId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}


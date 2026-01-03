using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/manager/approvals")]
[Authorize(Roles = "Manager,Admin")]
public class ManagerApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ManagerApprovalController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<PolicyApprovalDto>>> GetPendingApprovals()
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var approvals = await _approvalService.GetPendingApprovalsAsync(managerId);
            return Ok(approvals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("under-review")]
    public async Task<ActionResult<List<PolicyApprovalDto>>> GetUnderReview()
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var approvals = await _approvalService.GetPendingApprovalsAsync(managerId);
            var underReview = approvals.Where(a => a.Status == "UnderReview").ToList();
            return Ok(underReview);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<PolicyApprovalDto>>> GetHistory()
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var approvals = await _approvalService.GetPendingApprovalsAsync(managerId);
            var history = approvals.Where(a => a.Status == "Approved" || a.Status == "Rejected").ToList();
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApprovalStatisticsDto>> GetStatistics()
    {
        try
        {
            var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var statistics = await _approvalService.GetApprovalStatisticsAsync(managerId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}


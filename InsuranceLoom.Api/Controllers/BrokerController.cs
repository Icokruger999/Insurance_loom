using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/broker")]
[Authorize(Roles = "Broker")]
public class BrokerController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public BrokerController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpPost("policies/{policyId}/submit")]
    public async Task<ActionResult<PolicyApprovalDto>> SubmitPolicy(Guid policyId, [FromBody] SubmitPolicyRequest? request = null)
    {
        try
        {
            var brokerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var submitRequest = request ?? new SubmitPolicyRequest { PolicyId = policyId };
            var approval = await _approvalService.SubmitPolicyForApprovalAsync(submitRequest, brokerId);
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

    [HttpGet("policies/{policyId}/approval-status")]
    public async Task<ActionResult<PolicyApprovalDto>> GetApprovalStatus(Guid policyId)
    {
        try
        {
            var approval = await _approvalService.GetApprovalByPolicyIdAsync(policyId);
            if (approval == null)
                return NotFound(new { message = "Approval not found for this policy" });

            return Ok(approval);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}


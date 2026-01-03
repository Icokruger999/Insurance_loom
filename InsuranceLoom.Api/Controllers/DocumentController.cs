using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<DocumentUploadResponse>> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] DocumentUploadRequest request)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required" });

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var response = await _documentService.UploadDocumentAsync(file, request, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during upload", error = ex.Message });
        }
    }

    [HttpGet("required")]
    public async Task<ActionResult<RequiredDocumentsResponse>> GetRequiredDocuments(
        [FromQuery] string serviceType,
        [FromQuery] Guid? policyHolderId)
    {
        try
        {
            var response = await _documentService.GetRequiredDocumentsAsync(serviceType, policyHolderId);
            return Ok(response);
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

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments(
        [FromQuery] Guid? policyHolderId,
        [FromQuery] Guid? policyId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsAsync(policyHolderId, policyId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult> DownloadDocument(Guid id, [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var url = await _documentService.GetDocumentDownloadUrlAsync(id, expirationMinutes);
            return Ok(new { downloadUrl = url });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPut("{id}/verify")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<ActionResult> VerifyDocument(
        Guid id,
        [FromBody] VerifyDocumentRequest request)
    {
        try
        {
            var verifiedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
            var result = await _documentService.VerifyDocumentAsync(id, verifiedBy, request.RejectionReason);
            
            if (!result)
                return NotFound(new { message = "Document not found" });

            return Ok(new { message = request.RejectionReason == null ? "Document verified" : "Document rejected" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var result = await _documentService.DeleteDocumentAsync(id);
            if (!result)
                return NotFound(new { message = "Document not found" });

            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}

public class VerifyDocumentRequest
{
    public string? RejectionReason { get; set; }
}


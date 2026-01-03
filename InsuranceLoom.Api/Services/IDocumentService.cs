using InsuranceLoom.Api.Models.DTOs;

namespace InsuranceLoom.Api.Services;

public interface IDocumentService
{
    Task<DocumentUploadResponse> UploadDocumentAsync(IFormFile file, DocumentUploadRequest request, Guid uploadedBy);
    Task<RequiredDocumentsResponse> GetRequiredDocumentsAsync(string serviceType, Guid? policyHolderId);
    Task<List<DocumentDto>> GetDocumentsAsync(Guid? policyHolderId, Guid? policyId);
    Task<string> GetDocumentDownloadUrlAsync(Guid documentId, int expirationMinutes = 60);
    Task<bool> VerifyDocumentAsync(Guid documentId, Guid verifiedBy, string? rejectionReason = null);
    Task<bool> DeleteDocumentAsync(Guid documentId);
}


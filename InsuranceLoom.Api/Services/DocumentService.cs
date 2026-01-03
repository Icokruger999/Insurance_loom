using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceLoom.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IS3Service _s3Service;

    public DocumentService(ApplicationDbContext context, IS3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
    }

    public async Task<DocumentUploadResponse> UploadDocumentAsync(IFormFile file, DocumentUploadRequest request, Guid uploadedBy)
    {
        // Validate file
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        // Get document type
        var documentType = await _context.DocumentTypes
            .FirstOrDefaultAsync(dt => dt.DocumentCode == request.DocumentType && dt.IsActive);

        if (documentType == null)
            throw new ArgumentException("Invalid document type");

        // Validate file size
        var maxSizeBytes = documentType.MaxFileSizeMb * 1024 * 1024;
        if (file.Length > maxSizeBytes)
            throw new ArgumentException($"File size exceeds maximum of {documentType.MaxFileSizeMb}MB");

        // Validate file type
        var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
        if (!documentType.AllowedFileTypes.Contains(fileExtension))
            throw new ArgumentException($"File type .{fileExtension} is not allowed. Allowed types: {string.Join(", ", documentType.AllowedFileTypes)}");

        // Generate file name and path
        var fileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
        var folderPath = request.PolicyHolderId.HasValue 
            ? $"policy-holders/{request.PolicyHolderId.Value}" 
            : "temp";
        var s3Path = $"{folderPath}/{documentType.DocumentCode.ToLower()}/{fileName}";

        // Calculate checksum
        using var stream = file.OpenReadStream();
        var checksum = CalculateChecksum(stream);
        stream.Position = 0;

        // Upload to S3
        var filePath = await _s3Service.UploadFileAsync(stream, fileName, $"{folderPath}/{documentType.DocumentCode.ToLower()}");

        // Calculate expiry date
        DateTime? expiryDate = null;
        if (documentType.ValidityPeriodDays.HasValue)
        {
            expiryDate = DateTime.UtcNow.AddDays(documentType.ValidityPeriodDays.Value);
        }

        // Create document record
        var document = new Document
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = documentType.Id,
            PolicyHolderId = request.PolicyHolderId,
            PolicyId = request.PolicyId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = filePath,
            FileSizeBytes = file.Length,
            FileType = file.ContentType,
            FileExtension = fileExtension,
            UploadedBy = uploadedBy,
            UploadDate = DateTime.UtcNow,
            ExpiryDate = expiryDate,
            IsExpired = false,
            Status = "Pending",
            IsEncrypted = true,
            Checksum = checksum
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return new DocumentUploadResponse
        {
            DocumentId = document.Id,
            FileName = document.FileName,
            FileSize = document.FileSizeBytes,
            DocumentType = documentType.DocumentCode,
            Status = document.Status,
            UploadDate = document.UploadDate,
            ExpiryDate = document.ExpiryDate,
            Message = "Document uploaded successfully. Awaiting verification."
        };
    }

    public async Task<RequiredDocumentsResponse> GetRequiredDocumentsAsync(string serviceType, Guid? policyHolderId)
    {
        var service = await _context.ServiceTypes
            .FirstOrDefaultAsync(st => st.ServiceCode == serviceType && st.IsActive);

        if (service == null)
            throw new ArgumentException("Invalid service type");

        var requirements = await _context.ServiceDocumentRequirements
            .Include(sdr => sdr.DocumentType)
            .Where(sdr => sdr.ServiceTypeId == service.Id)
            .OrderBy(sdr => sdr.DisplayOrder)
            .ToListAsync();

        var requiredDocs = new List<RequiredDocumentDto>();
        var optionalDocs = new List<RequiredDocumentDto>();

        foreach (var req in requirements)
        {
            var docDto = new RequiredDocumentDto
            {
                DocumentType = req.DocumentType.DocumentCode,
                DocumentName = req.DocumentType.DocumentName,
                IsRequired = req.IsRequired,
                IsConditional = req.IsConditional,
                ConditionDescription = req.ConditionDescription,
                MaxFileSizeMB = req.DocumentType.MaxFileSizeMb,
                AllowedFileTypes = req.DocumentType.AllowedFileTypes,
                ValidityPeriodDays = req.DocumentType.ValidityPeriodDays
            };

            // Check if document is uploaded
            if (policyHolderId.HasValue)
            {
                var uploadedDoc = await _context.Documents
                    .FirstOrDefaultAsync(d => d.PolicyHolderId == policyHolderId 
                        && d.DocumentTypeId == req.DocumentType.Id 
                        && d.Status != "Rejected");

                if (uploadedDoc != null)
                {
                    docDto.IsUploaded = true;
                    docDto.DocumentId = uploadedDoc.Id;
                    docDto.Status = uploadedDoc.Status;
                }
            }

            if (req.IsRequired)
                requiredDocs.Add(docDto);
            else
                optionalDocs.Add(docDto);
        }

        return new RequiredDocumentsResponse
        {
            ServiceType = serviceType,
            RequiredDocuments = requiredDocs,
            OptionalDocuments = optionalDocs
        };
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync(Guid? policyHolderId, Guid? policyId)
    {
        var query = _context.Documents
            .Include(d => d.DocumentType)
            .AsQueryable();

        if (policyHolderId.HasValue)
            query = query.Where(d => d.PolicyHolderId == policyHolderId);

        if (policyId.HasValue)
            query = query.Where(d => d.PolicyId == policyId);

        var documents = await query
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        return documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType.DocumentCode,
            DocumentName = d.DocumentType.DocumentName,
            FileName = d.OriginalFileName,
            FileSizeBytes = d.FileSizeBytes,
            Status = d.Status,
            UploadDate = d.UploadDate,
            ExpiryDate = d.ExpiryDate,
            IsExpired = d.IsExpired,
            RejectionReason = d.RejectionReason
        }).ToList();
    }

    public async Task<string> GetDocumentDownloadUrlAsync(Guid documentId, int expirationMinutes = 60)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null)
            throw new ArgumentException("Document not found");

        return await _s3Service.GetSignedUrlAsync(document.FilePath, expirationMinutes);
    }

    public async Task<bool> VerifyDocumentAsync(Guid documentId, Guid verifiedBy, string? rejectionReason = null)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null)
            return false;

        if (string.IsNullOrEmpty(rejectionReason))
        {
            document.Status = "Verified";
            document.VerifiedBy = verifiedBy;
            document.VerifiedDate = DateTime.UtcNow;
        }
        else
        {
            document.Status = "Rejected";
            document.RejectionReason = rejectionReason;
        }

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null)
            return false;

        await _s3Service.DeleteFileAsync(document.FilePath);
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    private string CalculateChecksum(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}


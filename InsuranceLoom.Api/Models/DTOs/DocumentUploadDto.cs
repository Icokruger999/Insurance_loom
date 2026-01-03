namespace InsuranceLoom.Api.Models.DTOs;

public class DocumentUploadRequest
{
    public string DocumentType { get; set; } = string.Empty; // ID_DOCUMENT, PROOF_OF_ADDRESS, etc.
    public Guid? PolicyHolderId { get; set; }
    public Guid? PolicyId { get; set; }
    public string? Description { get; set; }
}

public class DocumentUploadResponse
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RequiredDocumentDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsUploaded { get; set; }
    public Guid? DocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MaxFileSizeMB { get; set; }
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    public int? ValidityPeriodDays { get; set; }
    public bool IsConditional { get; set; }
    public string? ConditionDescription { get; set; }
}

public class RequiredDocumentsResponse
{
    public string ServiceType { get; set; } = string.Empty;
    public List<RequiredDocumentDto> RequiredDocuments { get; set; } = new();
    public List<RequiredDocumentDto> OptionalDocuments { get; set; } = new();
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public string? RejectionReason { get; set; }
}


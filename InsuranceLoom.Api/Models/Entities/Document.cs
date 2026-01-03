using System.Text.Json;

namespace InsuranceLoom.Api.Models.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public Guid? PolicyHolderId { get; set; }
    public PolicyHolder? PolicyHolder { get; set; }
    public Guid? PolicyId { get; set; }
    public Policy? Policy { get; set; }
    public Guid? BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public Guid? UploadedBy { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; } = false;
    public string Status { get; set; } = "Pending"; // Pending, Verified, Rejected, Expired
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public string? Checksum { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string[]? Tags { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


namespace InsuranceLoom.Api.Models.Entities;

public class DocumentType
{
    public Guid Id { get; set; }
    public string DocumentCode { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsOptional { get; set; } = false;
    public int MaxFileSizeMb { get; set; } = 10;
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    public int? ValidityPeriodDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


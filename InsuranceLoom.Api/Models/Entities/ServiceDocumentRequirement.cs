namespace InsuranceLoom.Api.Models.Entities;

public class ServiceDocumentRequirement
{
    public Guid Id { get; set; }
    public Guid ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }
    public Guid DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsConditional { get; set; } = false;
    public string? ConditionDescription { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


namespace InsuranceLoom.Api.Models.DTOs;

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}


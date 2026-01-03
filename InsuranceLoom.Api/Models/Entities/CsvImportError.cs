using System.Text.Json;

namespace InsuranceLoom.Api.Models.Entities;

public class CsvImportError
{
    public Guid Id { get; set; }
    public Guid ImportId { get; set; }
    public CsvImport? Import { get; set; }
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public JsonDocument? RowData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


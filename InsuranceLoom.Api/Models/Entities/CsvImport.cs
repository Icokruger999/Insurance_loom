using System.Text.Json;

namespace InsuranceLoom.Api.Models.Entities;

public class CsvImport
{
    public Guid Id { get; set; }
    public Guid? BrokerId { get; set; }
    public Broker? Broker { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; } = 0;
    public int FailedRecords { get; set; } = 0;
    public string Status { get; set; } = "Processing"; // Processing, Completed, Failed, Partial
    public JsonDocument? ErrorLog { get; set; }
    public Guid? ImportedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}


using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/application")]
[Authorize]
public class ApplicationController : ControllerBase
{
    private readonly IEmailService _emailService;

    public ApplicationController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("email-pdf")]
    public async Task<IActionResult> EmailApplicationPDF([FromBody] EmailApplicationRequest request)
    {
        try
        {
            if (request.Recipients == null || request.Recipients.Length == 0)
            {
                return BadRequest(new { message = "At least one recipient email address is required" });
            }

            // Generate PDF HTML content
            var pdfHtml = GenerateApplicationPDFHtml(request.FormData);

            // Create PDF using html2pdf or similar library
            // For now, we'll send the HTML content in the email body
            // In production, you might want to use a library like iTextSharp or DinkToPdf to generate actual PDF bytes

            var subject = $"Insurance Application Form - {request.FormData.LastName}, {request.FormData.FirstName}";
            var body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px;"">
                    <h2>Insurance Application Form</h2>
                    <p>Please find the insurance application form attached.</p>
                    {pdfHtml}
                </div>
            ";

            // Send email to all recipients using the multi-recipient method
            await _emailService.SendEmailToMultipleRecipientsAsync(request.Recipients, subject, body, true);

            return Ok(new { message = $"Application PDF sent successfully to {request.Recipients.Length} recipient(s)" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error sending email", error = ex.Message });
        }
    }

    private string GenerateApplicationPDFHtml(ApplicationFormData formData)
    {
        var beneficiariesHtml = formData.Beneficiaries != null && formData.Beneficiaries.Length > 0
            ? $@"
                <table style=""width: 100%; border-collapse: collapse; margin-top: 1rem;"">
                    <thead>
                        <tr style=""background-color: #f5f5f5;"">
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Full Name</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Date of Birth</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Age</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Mobile</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Email</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Relationship</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">Type</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", formData.Beneficiaries.Select(b => $@"
                            <tr>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.FullName ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.DateOfBirth ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.Age?.ToString() ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.Mobile ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.Email ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.Relationship ?? ""}</td>
                                <td style=""border: 1px solid #ddd; padding: 8px;"">{b.Type ?? ""}</td>
                            </tr>
                        "))}
                    </tbody>
                </table>
            "
            : "<p>No beneficiaries added.</p>";

        return $@"
            <h3 style=""border-bottom: 2px solid #333; padding-bottom: 10px; margin-top: 30px;"">Personal Information</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr><td style=""padding: 8px; font-weight: bold; width: 40%;"">Last Name:</td><td style=""padding: 8px;"">{formData.LastName ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">First Name:</td><td style=""padding: 8px;"">{formData.FirstName ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Middle Name:</td><td style=""padding: 8px;"">{formData.MiddleName ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">ID Number:</td><td style=""padding: 8px;"">{formData.IdNumber ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Date of Birth:</td><td style=""padding: 8px;"">{formData.DateOfBirth ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Birthplace:</td><td style=""padding: 8px;"">{formData.Birthplace ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Sex:</td><td style=""padding: 8px;"">{formData.Sex ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Civil Status:</td><td style=""padding: 8px;"">{formData.CivilStatus ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Occupation:</td><td style=""padding: 8px;"">{formData.Occupation ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Email:</td><td style=""padding: 8px;"">{formData.Email ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Phone:</td><td style=""padding: 8px;"">{formData.Phone ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Address:</td><td style=""padding: 8px;"">{formData.Address ?? ""}</td></tr>
            </table>
            
            <h3 style=""border-bottom: 2px solid #333; padding-bottom: 10px; margin-top: 30px;"">Insurance Product Selection</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr><td style=""padding: 8px; font-weight: bold; width: 40%;"">Insurance Product:</td><td style=""padding: 8px;"">{formData.ServiceType ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Coverage Amount:</td><td style=""padding: 8px;"">{(formData.CoverageAmount.HasValue ? "R" + formData.CoverageAmount.Value.ToString("F2") : "")}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Premium Amount:</td><td style=""padding: 8px;"">{(formData.PremiumAmount.HasValue ? "R" + formData.PremiumAmount.Value.ToString("F2") : "")}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Activation Date:</td><td style=""padding: 8px;"">{formData.ActivationDate ?? ""}</td></tr>
            </table>
            
            <h3 style=""border-bottom: 2px solid #333; padding-bottom: 10px; margin-top: 30px;"">Financial Information</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr><td style=""padding: 8px; font-weight: bold; width: 40%;"">Monthly Income:</td><td style=""padding: 8px;"">{(formData.MonthlyIncome.HasValue ? "R" + formData.MonthlyIncome.Value.ToString("F2") : "")}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Monthly Expenses:</td><td style=""padding: 8px;"">{(formData.MonthlyExpenses.HasValue ? "R" + formData.MonthlyExpenses.Value.ToString("F2") : "")}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Payment Date:</td><td style=""padding: 8px;"">{formData.PaymentDate?.ToString() ?? ""}</td></tr>
            </table>
            
            <h3 style=""border-bottom: 2px solid #333; padding-bottom: 10px; margin-top: 30px;"">Beneficiaries</h3>
            {beneficiariesHtml}
            
            {(string.IsNullOrEmpty(formData.AgencyName) ? "" : $@"
            <h3 style=""border-bottom: 2px solid #333; padding-bottom: 10px; margin-top: 30px;"">Agency/Payor Information</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr><td style=""padding: 8px; font-weight: bold; width: 40%;"">Employment Start Date:</td><td style=""padding: 8px;"">{formData.EmploymentStartDate ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Employment End Date:</td><td style=""padding: 8px;"">{formData.EmploymentEndDate ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Agency Name:</td><td style=""padding: 8px;"">{formData.AgencyName ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Agency Contact:</td><td style=""padding: 8px;"">{formData.AgencyContactNo ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Agency Address:</td><td style=""padding: 8px;"">{formData.AgencyAddress ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Agency Email:</td><td style=""padding: 8px;"">{formData.AgencyEmail ?? ""}</td></tr>
                <tr><td style=""padding: 8px; font-weight: bold;"">Authorized Signatory:</td><td style=""padding: 8px;"">{formData.AgencySignatory ?? ""}</td></tr>
            </table>
            ")}
        ";
    }
}

public class EmailApplicationRequest
{
    public ApplicationFormData FormData { get; set; } = new();
    public string[] Recipients { get; set; } = Array.Empty<string>();
}

public class ApplicationFormData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Birthplace { get; set; }
    public string? Sex { get; set; }
    public string? CivilStatus { get; set; }
    public string? Occupation { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public string? ActivationDate { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? MonthlyExpenses { get; set; }
    public int? PaymentDate { get; set; }
    public BeneficiaryData[]? Beneficiaries { get; set; }
    public string? EmploymentStartDate { get; set; }
    public string? EmploymentEndDate { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyContactNo { get; set; }
    public string? AgencyAddress { get; set; }
    public string? AgencyEmail { get; set; }
    public string? AgencySignatory { get; set; }
}

public class BeneficiaryData
{
    public string FullName { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Relationship { get; set; }
    public string? Type { get; set; }
}


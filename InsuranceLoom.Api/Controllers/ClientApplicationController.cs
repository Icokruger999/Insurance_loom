using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/client/application")]
[Authorize(Roles = "PolicyHolder")]
public class ClientApplicationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBrokerAssignmentService _brokerAssignmentService;
    private readonly IEmailService _emailService;
    private readonly IDocumentService _documentService;

    public ClientApplicationController(
        ApplicationDbContext context,
        IBrokerAssignmentService brokerAssignmentService,
        IEmailService emailService,
        IDocumentService documentService)
    {
        _context = context;
        _brokerAssignmentService = brokerAssignmentService;
        _emailService = emailService;
        _documentService = documentService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitApplication([FromForm] CreatePolicyHolderRequest request)
    {
        try
        {
            // Get authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.UserType != "PolicyHolder")
            {
                return Unauthorized(new { message = "Invalid user type" });
            }

            // Get or create policy holder
            var policyHolder = await _context.PolicyHolders
                .FirstOrDefaultAsync(ph => ph.UserId == userId);

            if (policyHolder == null)
            {
                // Create policy holder if it doesn't exist
                policyHolder = new PolicyHolder
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PolicyNumber = "", // Will be set when policy is created
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    IdNumber = request.IdNumber,
                    Phone = request.Phone,
                    Address = request.Address,
                    DateOfBirth = request.DateOfBirth,
                    Birthplace = request.Birthplace,
                    Sex = request.Sex,
                    CivilStatus = request.CivilStatus,
                    Occupation = request.Occupation,
                    MonthlyIncome = request.MonthlyIncome,
                    MonthlyExpenses = request.MonthlyExpenses,
                    EmploymentType = request.EmploymentType,
                    IncomeTaxNumber = request.IncomeTaxNumber,
                    EmploymentStartDate = request.EmploymentStartDate,
                    EmploymentEndDate = request.EmploymentEndDate,
                    AgencyName = request.AgencyName,
                    AgencyContactNo = request.AgencyContactNo,
                    AgencyAddress = request.AgencyAddress,
                    AgencyEmail = request.AgencyEmail,
                    AgencySignatory = request.AgencySignatory,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PolicyHolders.Add(policyHolder);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing policy holder
                policyHolder.FirstName = request.FirstName ?? policyHolder.FirstName;
                policyHolder.LastName = request.LastName ?? policyHolder.LastName;
                policyHolder.MiddleName = request.MiddleName ?? policyHolder.MiddleName;
                policyHolder.IdNumber = request.IdNumber ?? policyHolder.IdNumber;
                policyHolder.Phone = request.Phone ?? policyHolder.Phone;
                policyHolder.Address = request.Address ?? policyHolder.Address;
                policyHolder.DateOfBirth = request.DateOfBirth ?? policyHolder.DateOfBirth;
                policyHolder.Birthplace = request.Birthplace ?? policyHolder.Birthplace;
                policyHolder.Sex = request.Sex ?? policyHolder.Sex;
                policyHolder.CivilStatus = request.CivilStatus ?? policyHolder.CivilStatus;
                policyHolder.Occupation = request.Occupation ?? policyHolder.Occupation;
                policyHolder.MonthlyIncome = request.MonthlyIncome ?? policyHolder.MonthlyIncome;
                policyHolder.MonthlyExpenses = request.MonthlyExpenses ?? policyHolder.MonthlyExpenses;
                policyHolder.EmploymentType = request.EmploymentType ?? policyHolder.EmploymentType;
                policyHolder.IncomeTaxNumber = request.IncomeTaxNumber ?? policyHolder.IncomeTaxNumber;
                policyHolder.EmploymentStartDate = request.EmploymentStartDate ?? policyHolder.EmploymentStartDate;
                policyHolder.EmploymentEndDate = request.EmploymentEndDate ?? policyHolder.EmploymentEndDate;
                policyHolder.AgencyName = request.AgencyName ?? policyHolder.AgencyName;
                policyHolder.AgencyContactNo = request.AgencyContactNo ?? policyHolder.AgencyContactNo;
                policyHolder.AgencyAddress = request.AgencyAddress ?? policyHolder.AgencyAddress;
                policyHolder.AgencyEmail = request.AgencyEmail ?? policyHolder.AgencyEmail;
                policyHolder.AgencySignatory = request.AgencySignatory ?? policyHolder.AgencySignatory;
                policyHolder.UpdatedAt = DateTime.UtcNow;
            }

            // Validate service type
            if (!request.ServiceTypeId.HasValue)
            {
                return BadRequest(new { message = "Service type is required" });
            }

            var serviceType = await _context.ServiceTypes.FindAsync(request.ServiceTypeId.Value);
            if (serviceType == null || !serviceType.IsActive)
            {
                return BadRequest(new { message = "Invalid service type" });
            }

            // Auto-assign broker
            Guid brokerId;
            try
            {
                brokerId = await _brokerAssignmentService.AssignBrokerAsync();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }

            // Generate policy number
            string policyNumber = await GenerateUniquePolicyNumberAsync();

            // Update policy holder policy number if empty
            if (string.IsNullOrEmpty(policyHolder.PolicyNumber))
            {
                policyHolder.PolicyNumber = policyNumber;
            }

            // Create policy
            var activationDate = request.ActivationDate ?? DateTime.UtcNow;
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = policyNumber,
                PolicyHolderId = policyHolder.Id,
                BrokerId = brokerId,
                ServiceTypeId = request.ServiceTypeId.Value,
                ServiceCode = serviceType.ServiceCode,
                CoverageAmount = request.CoverageAmount,
                PremiumAmount = request.PremiumAmount,
                StartDate = activationDate,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(policy);

            // Create debit order if payment details provided
            if (request.PaymentDate.HasValue && request.PremiumAmount.HasValue)
            {
                var today = DateTime.UtcNow;
                var nextDebitDate = new DateTime(today.Year, today.Month, request.PaymentDate.Value);
                if (nextDebitDate <= today)
                {
                    nextDebitDate = nextDebitDate.AddMonths(1);
                }

                var debitOrder = new DebitOrder
                {
                    Id = Guid.NewGuid(),
                    PolicyId = policy.Id,
                    PolicyHolderId = policyHolder.Id,
                    BankName = null,
                    BankAccountNumber = null,
                    Amount = request.PremiumAmount.Value,
                    Frequency = "Monthly",
                    NextDebitDate = nextDebitDate,
                    Status = "Active",
                    PaymentDay = request.PaymentDate.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DebitOrders.Add(debitOrder);
            }

            // Create beneficiaries if provided
            if (request.Beneficiaries != null && request.Beneficiaries.Any())
            {
                foreach (var beneficiaryData in request.Beneficiaries)
                {
                    var beneficiary = new Beneficiary
                    {
                        Id = Guid.NewGuid(),
                        PolicyHolderId = policyHolder.Id,
                        PolicyId = policy.Id,
                        FullName = beneficiaryData.FullName,
                        DateOfBirth = !string.IsNullOrEmpty(beneficiaryData.DateOfBirth) ? DateTime.Parse(beneficiaryData.DateOfBirth) : null,
                        Age = beneficiaryData.Age,
                        Mobile = beneficiaryData.Mobile,
                        Email = beneficiaryData.Email,
                        Relationship = beneficiaryData.Relationship,
                        Type = beneficiaryData.Type ?? "Revocable",
                        IsPrimary = beneficiaryData.IsPrimary,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Beneficiaries.Add(beneficiary);
                }
            }

            await _context.SaveChangesAsync();

            // Upload documents
            if (request.IdDocument != null && request.IdDocument.Length > 0)
            {
                var idDocRequest = new DocumentUploadRequest
                {
                    DocumentType = "ID_DOCUMENT",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy.Id,
                    Description = "ID Document uploaded during online application"
                };
                await _documentService.UploadDocumentAsync(request.IdDocument, idDocRequest, userId);
            }

            if (request.ProofOfAddress != null && request.ProofOfAddress.Length > 0)
            {
                var poaRequest = new DocumentUploadRequest
                {
                    DocumentType = "PROOF_OF_ADDRESS",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy.Id,
                    Description = "Proof of Address uploaded during online application"
                };
                await _documentService.UploadDocumentAsync(request.ProofOfAddress, poaRequest, userId);
            }

            if (request.BankStatement != null && request.BankStatement.Length > 0)
            {
                var bankStmtRequest = new DocumentUploadRequest
                {
                    DocumentType = "BANK_STATEMENT",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy.Id,
                    Description = "Bank Statement uploaded during online application"
                };
                await _documentService.UploadDocumentAsync(request.BankStatement, bankStmtRequest, userId);
            }

            // Send notification email to assigned broker
            try
            {
                var broker = await _context.Brokers
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == brokerId);
                
                if (broker?.User != null)
                {
                    await _emailService.SendEmailAsync(
                        broker.User.Email,
                        $"New Online Application Assigned - Policy {policyNumber}",
                        $@"
                            <h2>New Online Application Assigned</h2>
                            <p>A new online application has been assigned to you:</p>
                            <ul>
                                <li><strong>Policy Number:</strong> {policyNumber}</li>
                                <li><strong>Client:</strong> {policyHolder.FirstName} {policyHolder.LastName}</li>
                                <li><strong>Service Type:</strong> {serviceType.ServiceName}</li>
                                <li><strong>Coverage Amount:</strong> R {request.CoverageAmount?.ToString("N2") ?? "N/A"}</li>
                                <li><strong>Premium Amount:</strong> R {request.PremiumAmount?.ToString("N2") ?? "N/A"}</li>
                            </ul>
                            <p>Please review the application in your broker portal.</p>
                        "
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send broker notification email: {ex.Message}");
            }

            return Ok(new
            {
                message = "Application submitted successfully",
                policyId = policy.Id,
                policyNumber = policyNumber,
                brokerId = brokerId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while submitting the application", error = ex.Message });
        }
    }

    private async Task<string> GenerateUniquePolicyNumberAsync()
    {
        string policyNumber;
        bool isUnique = false;
        int attempts = 0;
        const int maxAttempts = 10;

        while (!isUnique && attempts < maxAttempts)
        {
            policyNumber = $"POL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            var exists = await _context.Policies.AnyAsync(p => p.PolicyNumber == policyNumber);
            if (!exists)
            {
                return policyNumber;
            }
            attempts++;
        }

        return $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(10000, 99999)}";
    }
}


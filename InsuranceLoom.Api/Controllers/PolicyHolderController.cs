using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using InsuranceLoom.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/policyholder")]
[Authorize]
public class PolicyHolderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentService _documentService;
    private readonly IEmailService _emailService;

    public PolicyHolderController(ApplicationDbContext context, IDocumentService documentService, IEmailService emailService)
    {
        _context = context;
        _documentService = documentService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<PolicyHolderDto>> RegisterPolicyHolder([FromForm] CreatePolicyHolderRequest request)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Get broker ID from authenticated user
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var broker = await _context.Brokers
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.User != null && b.User.Email == userEmail);

            if (broker == null || !broker.IsActive)
            {
                return Unauthorized(new { message = "Broker not found or inactive" });
            }

            // Generate unique policy number
            string policyNumber = await GenerateUniquePolicyNumberAsync();

            // Generate temporary password (client will reset it on first login)
            string tempPassword = GenerateTemporaryPassword();

            // Create user account with temporary password
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(tempPassword),
                UserType = "PolicyHolder",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Create policy holder
            var policyHolder = new PolicyHolder
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PolicyNumber = policyNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IdNumber = request.IdNumber,
                Phone = request.Phone,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PolicyHolders.Add(policyHolder);

            // Create policy if service type is provided
            Policy? policy = null;
            if (request.ServiceTypeId.HasValue)
            {
                var serviceType = await _context.ServiceTypes.FindAsync(request.ServiceTypeId.Value);
                if (serviceType == null || !serviceType.IsActive)
                {
                    return BadRequest(new { message = "Invalid service type" });
                }

                var activationDate = request.ActivationDate ?? DateTime.UtcNow;

                policy = new Policy
                {
                    Id = Guid.NewGuid(),
                    PolicyNumber = await GenerateUniquePolicyNumberAsync(),
                    PolicyHolderId = policyHolder.Id,
                    BrokerId = broker.Id,
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

                // Create debit order for payment (bank details will be added later)
                if (request.PaymentDate.HasValue && request.PremiumAmount.HasValue)
                {
                    // Calculate next debit date based on payment day
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
                        BankName = null, // Will be updated later
                        BankAccountNumber = null, // Will be updated later
                        Amount = request.PremiumAmount.Value,
                        Frequency = "Monthly",
                        NextDebitDate = nextDebitDate,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.DebitOrders.Add(debitOrder);
                }
            }

            await _context.SaveChangesAsync();

            // Upload documents
            var uploadedDocuments = new List<Guid>();
            var brokerUserId = broker.User?.Id ?? Guid.Empty;

            if (request.IdDocument != null && request.IdDocument.Length > 0)
            {
                var idDocRequest = new DocumentUploadRequest
                {
                    DocumentType = "ID_DOCUMENT",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy?.Id,
                    Description = "ID Document uploaded during registration"
                };
                var idDocResult = await _documentService.UploadDocumentAsync(request.IdDocument, idDocRequest, brokerUserId);
                uploadedDocuments.Add(idDocResult.DocumentId);
            }

            if (request.ProofOfAddress != null && request.ProofOfAddress.Length > 0)
            {
                var poaRequest = new DocumentUploadRequest
                {
                    DocumentType = "PROOF_OF_ADDRESS",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy?.Id,
                    Description = "Proof of Address uploaded during registration"
                };
                var poaResult = await _documentService.UploadDocumentAsync(request.ProofOfAddress, poaRequest, brokerUserId);
                uploadedDocuments.Add(poaResult.DocumentId);
            }

            if (request.BankStatement != null && request.BankStatement.Length > 0)
            {
                var bankStmtRequest = new DocumentUploadRequest
                {
                    DocumentType = "BANK_STATEMENT",
                    PolicyHolderId = policyHolder.Id,
                    PolicyId = policy?.Id,
                    Description = "Bank Statement uploaded during registration"
                };
                var bankStmtResult = await _documentService.UploadDocumentAsync(request.BankStatement, bankStmtRequest, brokerUserId);
                uploadedDocuments.Add(bankStmtResult.DocumentId);
            }

            // Send welcome email with temporary password
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Welcome to Insurance Loom - Your Account Has Been Created",
                    $@"
                        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
                            <div style=""background-color: #1a1a1a; color: #ffffff; padding: 20px; text-align: center;"">
                                <h1 style=""margin: 0;"">Insurance Loom</h1>
                            </div>
                            <div style=""background-color: #ffffff; padding: 30px;"">
                                <h2 style=""color: #333333;"">Welcome, {request.FirstName}!</h2>
                                <p style=""color: #666666; line-height: 1.6;"">Your account has been created by your broker.</p>
                                <div style=""background-color: #f8f9fa; padding: 20px; border-left: 4px solid #2563eb; margin: 20px 0;"">
                                    <p style=""margin: 0; color: #333333;""><strong>Your Policy Number:</strong> <span style=""font-size: 18px; color: #1a1a1a;"">{policyNumber}</span></p>
                                    <p style=""margin: 10px 0 0; color: #333333;""><strong>Temporary Password:</strong> <span style=""font-size: 16px; color: #1a1a1a; font-family: monospace;"">{tempPassword}</span></p>
                                </div>
                                <p style=""color: #666666; line-height: 1.6;"">Please log in using your email address and the temporary password above. You will be prompted to change your password on first login.</p>
                                <div style=""text-align: center; margin: 30px 0;"">
                                    <a href=""https://www.insuranceloom.com"" style=""display: inline-block; background-color: #1a1a1a; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px;"">Login to Your Account</a>
                                </div>
                                <p style=""color: #666666; line-height: 1.6; font-size: 14px; margin-top: 30px;"">Best regards,<br>The Insurance Loom Team</p>
                            </div>
                        </div>
                    "
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }

            return Ok(new PolicyHolderDto
            {
                Id = policyHolder.Id,
                PolicyNumber = policyHolder.PolicyNumber,
                FirstName = policyHolder.FirstName,
                LastName = policyHolder.LastName,
                Email = user.Email,
                Phone = policyHolder.Phone,
                IdNumber = policyHolder.IdNumber,
                Address = policyHolder.Address,
                DateOfBirth = policyHolder.DateOfBirth,
                IsActive = policyHolder.IsActive,
                CreatedAt = policyHolder.CreatedAt,
                PolicyId = policy?.Id,
                PolicyNumberGenerated = policy?.PolicyNumber,
                TemporaryPassword = tempPassword
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the policy holder", error = ex.Message });
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
            var exists = await _context.PolicyHolders.AnyAsync(p => p.PolicyNumber == policyNumber);
            if (!exists)
            {
                return policyNumber;
            }
            attempts++;
        }

        // Fallback with timestamp
        return $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(10000, 99999)}";
    }

    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

public class CreatePolicyHolderRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime? ActivationDate { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? MonthlyExpenses { get; set; }
    public int? PaymentDate { get; set; } // Day of month (1-28)
    public IFormFile? IdDocument { get; set; }
    public IFormFile? ProofOfAddress { get; set; }
    public IFormFile? BankStatement { get; set; }
}

public class PolicyHolderDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyNumberGenerated { get; set; }
    public string? TemporaryPassword { get; set; }
}

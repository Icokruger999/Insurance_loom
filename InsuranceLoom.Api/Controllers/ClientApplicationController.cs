using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using InsuranceLoom.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Controllers;

[ApiController]
[Route("api/client/application")]
[AllowAnonymous] // Allow anonymous submissions - applications will be assigned to brokers
public class ClientApplicationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBrokerAssignmentService _brokerAssignmentService;
    private readonly IEmailService _emailService;

    public ClientApplicationController(
        ApplicationDbContext context,
        IBrokerAssignmentService brokerAssignmentService,
        IEmailService emailService)
    {
        _context = context;
        _brokerAssignmentService = brokerAssignmentService;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitApplication([FromBody] AnonymousApplicationRequest request)
    {
        try
        {
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

            // Create policy holder without user account (anonymous submission)
            var policyHolder = new PolicyHolder
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty, // No user account for anonymous submissions
                PolicyNumber = policyNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IdNumber = request.IdNumber,
                Phone = request.Phone,
                StreetAddress = request.StreetName,
                City = request.Suburb,
                Province = request.Region,
                Country = "South Africa",
                DateOfBirth = request.DateOfBirth,
                MonthlyIncome = request.MonthlyIncome,
                MonthlyExpenses = request.MonthlyExpenses,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.PolicyHolders.Add(policyHolder);

            // Create policy with PendingSubmission status
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
                Status = "PendingSubmission", // Set to PendingSubmission so it appears in broker's pending applications
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
                    Status = "Pending", // Pending until bank details are added
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.DebitOrders.Add(debitOrder);
            }

            // Create PolicyApproval record for broker review
            var policyApproval = new PolicyApproval
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                BrokerId = brokerId,
                PolicyHolderId = policyHolder.Id,
                Status = "PendingSubmission",
                SubmittedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.PolicyApprovals.Add(policyApproval);

            // Log approval history
            _context.PolicyApprovalHistory.Add(new PolicyApprovalHistory
            {
                Id = Guid.NewGuid(),
                ApprovalId = policyApproval.Id,
                PolicyId = policy.Id,
                Action = "Submitted",
                PreviousStatus = "Draft",
                NewStatus = "PendingSubmission",
                PerformedByType = "Anonymous",
                Notes = "Online application submitted",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

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
                            <p>A new online application has been submitted and assigned to you:</p>
                            <ul>
                                <li><strong>Policy Number:</strong> {policyNumber}</li>
                                <li><strong>Client:</strong> {policyHolder.FirstName} {policyHolder.LastName}</li>
                                <li><strong>Email:</strong> {request.Email}</li>
                                <li><strong>Phone:</strong> {request.Phone}</li>
                                <li><strong>Service Type:</strong> {serviceType.ServiceName}</li>
                                <li><strong>Coverage Amount:</strong> R {request.CoverageAmount?.ToString("N2") ?? "N/A"}</li>
                                <li><strong>Premium Amount:</strong> R {request.PremiumAmount?.ToString("N2") ?? "N/A"}</li>
                            </ul>
                            <p>Please review the application in your broker portal under 'Pending Applications'.</p>
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
                message = "Application submitted successfully. A broker will review your application and contact you shortly.",
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

// DTO for anonymous application submissions
public class AnonymousApplicationRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? StreetName { get; set; }
    public string? Suburb { get; set; }
    public string? Region { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? PremiumAmount { get; set; }
    public DateTime? ActivationDate { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? MonthlyExpenses { get; set; }
    public int? PaymentDate { get; set; } // Day of month (1-31)
}

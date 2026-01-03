using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Helpers;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenGenerator _jwtGenerator;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, JwtTokenGenerator jwtGenerator, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<BrokerDto> RegisterBrokerAsync(CreateBrokerRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already exists");

        // Validate company name if provided - must exist in database
        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var companyExists = await _context.Companies
                .AnyAsync(c => c.Name.ToLower() == request.CompanyName.Trim().ToLower() && c.IsActive);
            
            if (!companyExists)
            {
                throw new ArgumentException($"Company '{request.CompanyName}' does not exist. Please select an existing company.");
            }
        }

        // Generate unique agent number
        string agentNumber = await GenerateUniqueAgentNumberAsync();

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            UserType = "Broker",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create broker with auto-generated agent number and default commission rate
        var broker = new Broker
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AgentNumber = agentNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Phone = request.Phone,
            LicenseNumber = request.LicenseNumber,
            CommissionRate = 0.00m, // Default 0% commission rate (can be updated later)
            IsActive = false, // Pending approval - will be set to true after admin approval
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Brokers.Add(broker);
        await _context.SaveChangesAsync();

        // Send registration confirmation email to broker (pending approval)
        try
        {
            await _emailService.SendBrokerRegistrationNotificationAsync(
                user.Email,
                agentNumber,
                broker.FirstName,
                broker.LastName
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send registration email: {ex.Message}");
        }

        // Send approval request email to approver
        try
        {
            var approverEmail = _configuration["BrokerApproval:ApproverEmail"];
            if (!string.IsNullOrEmpty(approverEmail))
            {
                await _emailService.SendBrokerApprovalRequestAsync(
                    approverEmail,
                    user.Email,
                    agentNumber,
                    broker.FirstName,
                    broker.LastName,
                    broker.CompanyName ?? "",
                    broker.Id
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send approval request email: {ex.Message}");
        }

        return new BrokerDto
        {
            Id = broker.Id,
            Email = user.Email,
            AgentNumber = broker.AgentNumber,
            FirstName = broker.FirstName,
            LastName = broker.LastName,
            CompanyName = broker.CompanyName,
            Phone = broker.Phone,
            LicenseNumber = broker.LicenseNumber,
            CommissionRate = broker.CommissionRate,
            IsActive = broker.IsActive,
            CreatedAt = broker.CreatedAt
        };
    }

    private async Task<string> GenerateUniqueAgentNumberAsync()
    {
        // Get the highest existing agent number
        var lastBroker = await _context.Brokers
            .Where(b => b.AgentNumber.StartsWith("AGT-"))
            .OrderByDescending(b => b.AgentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastBroker != null && !string.IsNullOrEmpty(lastBroker.AgentNumber))
        {
            // Extract number from format AGT-XXXXXX
            var parts = lastBroker.AgentNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        // Format as AGT-000001, AGT-000002, etc.
        string agentNumber = $"AGT-{nextNumber:D6}";

        // Double-check uniqueness (shouldn't happen, but safety check)
        var exists = await _context.Brokers.AnyAsync(b => b.AgentNumber == agentNumber);
        if (exists)
        {
            // If by some chance it exists, try next number
            nextNumber++;
            agentNumber = $"AGT-{nextNumber:D6}";
        }

        return agentNumber;
    }

    public async Task<LoginResponse?> BrokerLoginAsync(BrokerLoginRequest request)
    {
        var broker = await _context.Brokers
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.User != null && b.User.Email == request.Email);

        if (broker == null || broker.User == null || !broker.User.IsActive)
            return null;

        if (!PasswordHasher.VerifyPassword(request.Password, broker.User.PasswordHash))
            return null;

        // Check if broker is approved (IsActive must be true)
        if (!broker.IsActive)
            return null; // Broker is pending approval

        var token = _jwtGenerator.GenerateToken(broker.User.Id, broker.User.Email, "Broker");
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtGenerator.GetTokenExpiration(),
            User = new UserInfo
            {
                Id = broker.User.Id,
                Email = broker.User.Email,
                UserType = "Broker"
            },
            Broker = new BrokerInfo
            {
                Id = broker.Id,
                AgentNumber = broker.AgentNumber,
                FirstName = broker.FirstName,
                LastName = broker.LastName,
                CompanyName = broker.CompanyName,
                Email = broker.User.Email
            }
        };
    }

    public async Task<LoginResponse?> PolicyHolderLoginAsync(PolicyHolderLoginRequest request)
    {
        var policyHolder = await _context.PolicyHolders
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PolicyNumber == request.PolicyNumber && p.IsActive);

        if (policyHolder == null || policyHolder.User == null || !policyHolder.User.IsActive)
            return null;

        if (!PasswordHasher.VerifyPassword(request.Password, policyHolder.User.PasswordHash))
            return null;

        var token = _jwtGenerator.GenerateToken(policyHolder.User.Id, policyHolder.User.Email, "PolicyHolder");
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtGenerator.GetTokenExpiration(),
            User = new UserInfo
            {
                Id = policyHolder.User.Id,
                Email = policyHolder.User.Email,
                UserType = "PolicyHolder"
            },
            PolicyHolder = new PolicyHolderInfo
            {
                Id = policyHolder.Id,
                PolicyNumber = policyHolder.PolicyNumber,
                FirstName = policyHolder.FirstName,
                LastName = policyHolder.LastName,
                Email = policyHolder.User.Email
            }
        };
    }

    public async Task<LoginResponse?> ManagerLoginAsync(ManagerLoginRequest request)
    {
        var manager = await _context.Managers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Email == request.Email && m.IsActive);

        if (manager == null || manager.User == null || !manager.User.IsActive)
            return null;

        if (!PasswordHasher.VerifyPassword(request.Password, manager.User.PasswordHash))
            return null;

        var token = _jwtGenerator.GenerateToken(manager.User.Id, manager.User.Email, "Manager");
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtGenerator.GetTokenExpiration(),
            User = new UserInfo
            {
                Id = manager.User.Id,
                Email = manager.User.Email,
                UserType = "Manager"
            },
            Manager = new ManagerInfo
            {
                Id = manager.Id,
                Email = manager.Email,
                FirstName = manager.FirstName,
                LastName = manager.LastName,
                EmployeeNumber = manager.EmployeeNumber,
                Department = manager.Department,
                Permissions = new ManagerPermissions
                {
                    CanApprovePolicies = manager.CanApprovePolicies,
                    CanManageBrokers = manager.CanManageBrokers,
                    CanViewReports = manager.CanViewReports
                }
            }
        };
    }
}


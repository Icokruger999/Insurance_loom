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

    public AuthService(ApplicationDbContext context, JwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<BrokerDto> RegisterBrokerAsync(CreateBrokerRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already exists");

        // Check if agent number already exists
        var existingBroker = await _context.Brokers.FirstOrDefaultAsync(b => b.AgentNumber == request.AgentNumber);
        if (existingBroker != null)
            throw new ArgumentException("Agent number already exists");

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

        // Create broker
        var broker = new Broker
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AgentNumber = request.AgentNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Phone = request.Phone,
            LicenseNumber = request.LicenseNumber,
            CommissionRate = request.CommissionRate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Brokers.Add(broker);
        await _context.SaveChangesAsync();

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

    public async Task<LoginResponse?> BrokerLoginAsync(BrokerLoginRequest request)
    {
        var broker = await _context.Brokers
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.AgentNumber == request.AgentNumber && b.IsActive);

        if (broker == null || broker.User == null || !broker.User.IsActive)
            return null;

        if (!PasswordHasher.VerifyPassword(request.Password, broker.User.PasswordHash))
            return null;

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


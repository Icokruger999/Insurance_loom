using InsuranceLoom.Api.Data;
using InsuranceLoom.Api.Helpers;
using InsuranceLoom.Api.Models.DTOs;
using InsuranceLoom.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Services;

public class ManagerService : IManagerService
{
    private readonly ApplicationDbContext _context;

    public ManagerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ManagerDto> CreateManagerAsync(CreateManagerRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already exists");

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            UserType = "Manager",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create manager
        var manager = new Manager
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            EmployeeNumber = request.EmployeeNumber,
            Department = request.Department,
            IsActive = true,
            CanApprovePolicies = request.Permissions.CanApprovePolicies,
            CanManageBrokers = request.Permissions.CanManageBrokers,
            CanViewReports = request.Permissions.CanViewReports,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Managers.Add(manager);
        await _context.SaveChangesAsync();

        return new ManagerDto
        {
            Id = manager.Id,
            Email = manager.Email,
            FirstName = manager.FirstName,
            LastName = manager.LastName,
            Phone = manager.Phone,
            EmployeeNumber = manager.EmployeeNumber,
            Department = manager.Department,
            IsActive = manager.IsActive,
            Permissions = new ManagerPermissionsDto
            {
                CanApprovePolicies = manager.CanApprovePolicies,
                CanManageBrokers = manager.CanManageBrokers,
                CanViewReports = manager.CanViewReports
            },
            CreatedAt = manager.CreatedAt
        };
    }

    public async Task<List<ManagerDto>> GetAllManagersAsync()
    {
        var managers = await _context.Managers
            .Where(m => m.IsActive)
            .OrderBy(m => m.FirstName)
            .ThenBy(m => m.LastName)
            .ToListAsync();

        return managers.Select(m => new ManagerDto
        {
            Id = m.Id,
            Email = m.Email,
            FirstName = m.FirstName,
            LastName = m.LastName,
            Phone = m.Phone,
            EmployeeNumber = m.EmployeeNumber,
            Department = m.Department,
            IsActive = m.IsActive,
            Permissions = new ManagerPermissionsDto
            {
                CanApprovePolicies = m.CanApprovePolicies,
                CanManageBrokers = m.CanManageBrokers,
                CanViewReports = m.CanViewReports
            },
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<ManagerDto?> GetManagerByIdAsync(Guid id)
    {
        var manager = await _context.Managers.FindAsync(id);
        if (manager == null)
            return null;

        return new ManagerDto
        {
            Id = manager.Id,
            Email = manager.Email,
            FirstName = manager.FirstName,
            LastName = manager.LastName,
            Phone = manager.Phone,
            EmployeeNumber = manager.EmployeeNumber,
            Department = manager.Department,
            IsActive = manager.IsActive,
            Permissions = new ManagerPermissionsDto
            {
                CanApprovePolicies = manager.CanApprovePolicies,
                CanManageBrokers = manager.CanManageBrokers,
                CanViewReports = manager.CanViewReports
            },
            CreatedAt = manager.CreatedAt
        };
    }

    public async Task<bool> UpdateManagerAsync(Guid id, CreateManagerRequest request)
    {
        var manager = await _context.Managers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manager == null)
            return false;

        // Update user email if changed
        if (manager.User != null && manager.User.Email != request.Email)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != manager.UserId);
            if (emailExists)
                throw new ArgumentException("Email already exists");

            manager.User.Email = request.Email;
            manager.Email = request.Email;
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            if (manager.User != null)
                manager.User.PasswordHash = PasswordHasher.HashPassword(request.Password);
        }

        // Update manager details
        manager.FirstName = request.FirstName;
        manager.LastName = request.LastName;
        manager.Phone = request.Phone;
        manager.EmployeeNumber = request.EmployeeNumber;
        manager.Department = request.Department;
        manager.CanApprovePolicies = request.Permissions.CanApprovePolicies;
        manager.CanManageBrokers = request.Permissions.CanManageBrokers;
        manager.CanViewReports = request.Permissions.CanViewReports;
        manager.UpdatedAt = DateTime.UtcNow;

        if (manager.User != null)
            manager.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateManagerAsync(Guid id)
    {
        var manager = await _context.Managers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manager == null)
            return false;

        manager.IsActive = false;
        manager.UpdatedAt = DateTime.UtcNow;

        if (manager.User != null)
        {
            manager.User.IsActive = false;
            manager.User.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}


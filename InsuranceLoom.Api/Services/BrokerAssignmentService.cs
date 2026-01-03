using InsuranceLoom.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Services;

public class BrokerAssignmentService : IBrokerAssignmentService
{
    private readonly ApplicationDbContext _context;
    private static readonly object _lockObject = new object();
    private static int _lastAssignedIndex = -1;

    public BrokerAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AssignBrokerAsync()
    {
        // Get all active brokers
        var activeBrokers = await _context.Brokers
            .Include(b => b.User)
            .Where(b => b.IsActive && b.User != null && b.User.IsActive)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();

        if (!activeBrokers.Any())
        {
            throw new InvalidOperationException("No active brokers available for assignment");
        }

        // Round-robin assignment using thread-safe locking
        lock (_lockObject)
        {
            _lastAssignedIndex = (_lastAssignedIndex + 1) % activeBrokers.Count;
            return activeBrokers[_lastAssignedIndex].Id;
        }
    }
}


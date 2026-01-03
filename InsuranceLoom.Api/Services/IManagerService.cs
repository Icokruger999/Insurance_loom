using InsuranceLoom.Api.Models.DTOs;

namespace InsuranceLoom.Api.Services;

public interface IManagerService
{
    Task<ManagerDto> CreateManagerAsync(CreateManagerRequest request);
    Task<List<ManagerDto>> GetAllManagersAsync();
    Task<ManagerDto?> GetManagerByIdAsync(Guid id);
    Task<bool> UpdateManagerAsync(Guid id, CreateManagerRequest request);
    Task<bool> DeactivateManagerAsync(Guid id);
}


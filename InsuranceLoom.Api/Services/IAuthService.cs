using InsuranceLoom.Api.Models.DTOs;

namespace InsuranceLoom.Api.Services;

public interface IAuthService
{
    Task<BrokerDto> RegisterBrokerAsync(CreateBrokerRequest request);
    Task<LoginResponse?> BrokerLoginAsync(BrokerLoginRequest request);
    Task<LoginResponse?> PolicyHolderLoginAsync(PolicyHolderLoginRequest request);
    Task<LoginResponse?> ManagerLoginAsync(ManagerLoginRequest request);
}


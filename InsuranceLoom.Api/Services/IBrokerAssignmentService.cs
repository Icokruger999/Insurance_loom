namespace InsuranceLoom.Api.Services;

public interface IBrokerAssignmentService
{
    Task<Guid> AssignBrokerAsync();
}


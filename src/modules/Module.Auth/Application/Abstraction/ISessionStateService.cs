using Common.Contracts.authentication.dtos;
using Module.Auth.Domain;

namespace Module.Auth.Application.Abstraction;

public interface ISessionStateService
{
    Task<SessionStateDto> GetOrBuildAsync(Guid userId, Guid tenantId, UserType userType);
    void Invalidate(Guid userId);
    void InvalidateTenant(Guid tenantId);
}

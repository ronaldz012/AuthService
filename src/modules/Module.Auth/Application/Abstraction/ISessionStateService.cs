using Common.Contracts.authentication.dtos;

namespace Module.Auth.Application.Abstraction;

public interface ISessionStateService
{
    Task<SessionStateDto> GetOrBuildAsync(Guid userId, Guid tenantId, bool isAdmin);
    void Invalidate(Guid userId);
}

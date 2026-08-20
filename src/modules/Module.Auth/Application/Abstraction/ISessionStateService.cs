using Common.Contracts.authentication;
using Common.Contracts.authentication.dtos;
using Common.Utilities;

namespace Module.Auth.Application.Abstraction;

public interface ISessionStateService
{
    Task<Result<AuthenticatedSessionDto>> AuthenticateByExternalIdAsync(string externalAuthId);
    Result<SessionStateDto> GetSessionAsync();
    Result<ActorContext> GetActorContext();
    void Invalidate(string externalAuthId);
    void InvalidateTenant(Guid tenantId);
}

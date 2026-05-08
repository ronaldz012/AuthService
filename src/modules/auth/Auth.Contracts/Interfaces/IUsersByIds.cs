using Auth.Contracts.Dtos.Users;
using Common.Result;

namespace Auth.Contracts.Interfaces;

public interface IUserIntegrationService
{
    public Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<Guid> userIds);
    
    public Task<Result<Guid>> CreateTenantAdminAsync(string email, string password);
}
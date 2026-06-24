using Common.Utilities;
using module.Auth.dtos.Users;

namespace module.Auth.interfaces;

public interface IUserIntegrationService
{
    public Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<Guid> userIds);
    
    public Task<Result<Guid>> CreateTenantAdminAsync(string email, string password);
}
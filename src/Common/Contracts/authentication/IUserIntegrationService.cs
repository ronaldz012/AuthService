using Common.Contracts.authentication.dtos;
using Common.Utilities;

namespace Common.Contracts.authentication;

public interface IUserIntegrationService
{
    public Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<Guid> userIds);
    
}
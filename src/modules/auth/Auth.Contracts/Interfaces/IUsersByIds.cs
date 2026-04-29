using Auth.Contracts.Dtos.Users;
using Shared.Result;

namespace Auth.Contracts.Interfaces;

public interface IUserIntegrationService
{
    public Task<Result<List<UserDetailsDto>>> GetUsersByIds(List<int> userIds);
}
using Common.Utilities;

namespace Module.Auth.Application.Abstraction;

public interface IAuth0ProvisioningService
{
    Task<Result<string>> EnsureTestUserAsync(string email, string password);
    Task<Result<string>> CreateInvitationUserAsync(string email);
    Task<Result<bool>> SendInvitationAsync(string email);
    Task<Result<string>> CreatePasswordChangeTicketAsync(string auth0UserId, string? resultUrl = null, int ttlSec = 432000);
}
public static class Auth0ProvisioningErrors
{
    public static readonly Error TokenFetchFailed = new(ErrorCode.InternalError, "Failed to obtain Auth0 management token");
    public static readonly Error UserSearchFailed = new(ErrorCode.InternalError, "Failed to search Auth0 user");
    public static readonly Error UserCreationFailed = new(ErrorCode.InternalError, "Failed to create Auth0 user");
    public static readonly Error InvitationFailed = new(ErrorCode.InternalError, "Failed to send Auth0 invitation email");
}

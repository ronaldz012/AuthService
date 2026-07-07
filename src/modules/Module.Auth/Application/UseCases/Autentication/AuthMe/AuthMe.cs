using Common.Contracts.authentication;
using Common.Utilities;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Autentication.AuthMe;

public class AuthMe(
    ICurrentUser currentUser,
    ISessionStateService sessionState)
{
    public async Task<Result<AuthMeResponse>> Execute()
    {
        var session = await sessionState.GetOrBuildAsync(
            currentUser.UserId,
            currentUser.TenantId,
            currentUser.IsAdmin);

        return new AuthMeResponse(session);
    }
}

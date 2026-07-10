using Common.Contracts.authentication;
using Common.Utilities;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

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
            (UserType)currentUser.UserType);

        return new AuthMeResponse(session);
    }
}

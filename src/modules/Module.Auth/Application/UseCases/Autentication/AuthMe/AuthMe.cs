using Common.Contracts.authentication;
using Common.Utilities;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.Autentication.AuthMe;

public class AuthMe(ISessionStateService sessionState)
{
    public async Task<Result<AuthMeResponse>> Execute()
    {
        var result =  sessionState.GetSessionAsync();
        if (!result.IsSuccess)
            return result.Error;

        return new AuthMeResponse(result.Value);
    }
}

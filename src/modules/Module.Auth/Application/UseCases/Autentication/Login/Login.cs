using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Autentication.Login;

public class Login(
    IAuthDbContext dbContext,
    ITenantConnectionContext tenantConnectionContext,
    ISessionStateService sessionState,
    ITokenGenerator tokenGenerator,
    ILogger<Login> logger) 
{
    public async Task<Result<LoginResponse>> Execute(LoginRequest request)
    {
        var user = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Email);

        if (user == null)
            return LoginErrors.UserNotFound;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return LoginErrors.InvalidPassword;

        tenantConnectionContext.TenantId = user.TenantId;

        var session = await sessionState.GetOrBuildAsync(user.Id, user.TenantId, user.Type);

        var expirationMinutes = tokenGenerator.GetExpirationMinutes();
        var accessToken = tokenGenerator.GenerateAccessToken(
            user.Id,
            user.TenantId,
            user.Type);

        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return new LoginResponse(accessToken, refreshToken, expirationMinutes * 60, session);
    }
}

using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Infrastructure.Authentication;

namespace Module.Auth.Application.UseCases.Autentication.RefreshToken;

public class RefreshTokenUseCase(
    IRefreshTokenService refreshTokenService,
    ITokenGenerator tokenGenerator,
    IAuthDbContext dbContext)
{
    public async Task<Result<RefreshTokenResponse>> Execute(string refreshToken)
    {
        var tokenHash = RefreshTokenService.HashToken(refreshToken);

        var token = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (token is null || !token.IsActive)
            return RefreshTokenErrors.InvalidOrExpired;

        if (!token.User.IsActive)
            return RefreshTokenErrors.InactiveUser;

        var newRefreshToken = await refreshTokenService.RevokeAndGenerateAsync(token);

        var fullName = $"{token.User.FirstName} {token.User.LastName}".Trim();
        var accessToken = tokenGenerator.GenerateAccessToken(
            token.UserId, token.User.TenantId, token.User.Type, fullName);

        var expiresIn = tokenGenerator.GetExpirationMinutes() * 60;

        return new RefreshTokenResponse(accessToken, newRefreshToken);
    }
}
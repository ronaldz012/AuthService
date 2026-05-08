using System;

namespace Auth.Infrastructure.Authentication;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId, string tenant);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}

using System;

namespace Auth.Infrastructure.Authentication;

public interface ITokenGenerator
{
    string GenerateAccessToken(int userId, string tenant);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}

using System;

namespace Auth.Infrastructure.Authentication;

public interface ITokenGenerator
{
    string GenerateAccessToken(int userId);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}

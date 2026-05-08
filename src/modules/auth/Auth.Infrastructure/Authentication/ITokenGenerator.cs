using System;

namespace Auth.Infrastructure.Authentication;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId,Guid tenantId, string schema,string db,bool isAdmin);
    string GenerateRefreshToken();
    int GetAccessTokenExpirationMinutes();
}

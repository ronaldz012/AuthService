using Module.Auth.Domain;

namespace Module.Auth.Application.Abstraction;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId, Guid tenantId, UserType userType, string fullName);
    string GenerateRefreshToken();
    int GetExpirationMinutes();
}
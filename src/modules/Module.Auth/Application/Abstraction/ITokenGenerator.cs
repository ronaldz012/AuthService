namespace Module.Auth.Application.Abstraction;

public interface ITokenGenerator
{
    string GenerateAccessToken(Guid userId, Guid tenantId, string schema, bool isAdmin);
    string GenerateRefreshToken();
    int GetExpirationMinutes();
}
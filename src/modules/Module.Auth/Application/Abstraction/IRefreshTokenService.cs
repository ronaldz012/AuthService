using Module.Auth.Domain;

namespace Module.Auth.Application.Abstraction;

public interface IRefreshTokenService
{
    Task<string> GenerateAsync(Guid userId);
    Task<string> RevokeAndGenerateAsync(RefreshToken token);
    Task RevokeAsync(string refreshToken);
}
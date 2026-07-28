using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Authentication;

public class RefreshTokenService(IAuthDbContext context) : IRefreshTokenService
{
    private static readonly TimeSpan Expiration = TimeSpan.FromDays(30);

    public async Task<string> GenerateAsync(Guid userId)
    {
        var token = GenerateCryptographicToken();
        var tokenHash = HashToken(token);

        context.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(Expiration),
        });

        await context.SaveChangesAsync();
        return token;
    }

    public async Task<string> RevokeAndGenerateAsync(RefreshToken token)
    {
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        var newToken = GenerateCryptographicToken();
        var newHash = HashToken(newToken);

        context.Add(new RefreshToken
        {
            UserId = token.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(Expiration),
        });

        await context.SaveChangesAsync();
        return newToken;
    }

    private static string GenerateCryptographicToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
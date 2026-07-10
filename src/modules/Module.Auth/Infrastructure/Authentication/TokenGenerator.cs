using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Authentication;

public class JwtTokenGenerator(IConfiguration configuration) : ITokenGenerator
{
    public string GenerateAccessToken(Guid userId, Guid tenantId, UserType userType)
    {
        var keyString = configuration["TokenSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Key is missing in config.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenantId", tenantId.ToString()),
            new Claim("user_type", ((int)userType).ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["TokenSettings:Issuer"],
            audience: configuration["TokenSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpirationMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public int GetExpirationMinutes()
    {
        return int.TryParse(configuration["TokenSettings:ExpirationMinutes"], out var exp) ? exp : 60;
    }
}
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Module.Auth.Application.Abstraction;
using System.Security.Claims;

namespace Module.Auth.Infrastructure.Authentication;

public interface IAuth0TokenValidator
{
    Task<Result<string>> ValidateTokenAsync(string accessToken);
}

public class Auth0TokenValidator(IOptions<Auth0Settings> options, ILogger<Auth0TokenValidator> logger) : IAuth0TokenValidator
{
    private readonly Auth0Settings _settings = options.Value;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager = new(
        $"https://{options.Value.Domain}/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever(),
        new HttpDocumentRetriever { RequireHttps = true });

    public async Task<Result<string>> ValidateTokenAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Auth0Errors.InvalidToken;

        try
        {
            var openIdConfig = await _configManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = $"https://{_settings.Domain}/",
                ValidAudience = _settings.Audience,
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            var handler = new JsonWebTokenHandler();
            var result = handler.ValidateToken(accessToken, validationParameters);

            if (!result.IsValid)
            {
                logger.LogWarning(result.Exception, "Auth0 token invalid.");
                return Auth0Errors.InvalidToken;
            }

            var jwt = result.SecurityToken as JsonWebToken;

            if (!string.Equals(jwt?.Alg, SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Auth0 token algorithm rejected. Alg={Alg}", jwt?.Alg);
                return Auth0Errors.InvalidToken;
            }

            var subject = result.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? result.ClaimsIdentity.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                logger.LogWarning("Auth0 token has no subject claim.");
                return Auth0Errors.InvalidToken;
            }

            return subject;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auth0 token validation failed.");
            return Auth0Errors.InvalidToken;
        }
    }
}

public static class Auth0Errors
{
    public static readonly Error InvalidToken = new(ErrorCode.Unauthorized, "Auth0 token is invalid or expired");
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Module.Auth.Infrastructure.Authentication;

public interface IAuth0ProvisioningService
{
    Task<string> EnsureTestUserAsync(string email, string password);
}

public class Auth0ProvisioningService(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<Auth0ProvisioningService> logger) : IAuth0ProvisioningService
{
    private readonly string _domain = config["Auth0:Domain"] ?? string.Empty;
    private readonly string _clientId = config["Auth0:M2M:ClientId"] ?? string.Empty;
    private readonly string _clientSecret = config["Auth0:M2M:ClientSecret"] ?? string.Empty;

    public async Task<string> EnsureTestUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(_domain))
            throw new InvalidOperationException(
                "Auth0:Domain is not configured. Set Auth0:Domain in appsettings to enable Auth0 provisioning.");

        logger.LogInformation("Auth0 provisioning started for {Email}. Domain={Domain}", email, _domain);
        logger.LogInformation("M2M ClientId configured: {Configured}, ClientSecret configured: {SecretConfigured}",
            !string.IsNullOrWhiteSpace(_clientId), !string.IsNullOrWhiteSpace(_clientSecret));

        var token = await GetManagementApiTokenAsync();
        logger.LogInformation("Auth0 management token obtained. Length={Length}", token.Length);

        // 1. Verificar si ya existe
        using var searchRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://{_domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}");
        searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var searchResp = await httpClient.SendAsync(searchRequest);
        var searchBody = await searchResp.Content.ReadAsStringAsync();
        logger.LogInformation("Auth0 search users-by-email status={Status} body={Body}",
            (int)searchResp.StatusCode, Truncate(searchBody, 500));

        if (!searchResp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Auth0 search users-by-email failed: {(int)searchResp.StatusCode} {searchBody}");

        var existing = System.Text.Json.JsonSerializer.Deserialize<List<Auth0UserDto>>(searchBody);
        if (existing is { Count: > 0 })
        {
            logger.LogInformation("Auth0 user already exists for {Email}: {UserId}", email, existing[0].UserId);
            return existing[0].UserId;
        }

        logger.LogInformation("Auth0 user {Email} not found. Creating it...", email);

        // 2. Crear si no existe
        var body = new
        {
            email,
            password,
            connection = "Username-Password-Authentication",
            email_verified = true
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"https://{_domain}/api/v2/users")
        {
            Content = JsonContent.Create(body)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await httpClient.SendAsync(createRequest);
        var createBody = await createResp.Content.ReadAsStringAsync();
        logger.LogInformation("Auth0 create user status={Status} body={Body}",
            (int)createResp.StatusCode, Truncate(createBody, 500));

        if (!createResp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Auth0 create user failed: {(int)createResp.StatusCode} {createBody}");

        var created = System.Text.Json.JsonSerializer.Deserialize<Auth0UserDto>(createBody);
        logger.LogInformation("Auth0 user created for {Email}: {UserId}", email, created?.UserId);
        return created!.UserId;
    }

    private async Task<string> GetManagementApiTokenAsync()
    {
        var body = new
        {
            grant_type = "client_credentials",
            client_id = _clientId,
            client_secret = _clientSecret,
            audience = $"https://{_domain}/api/v2/"
        };

        logger.LogInformation("Requesting Auth0 management token from https://{Domain}/oauth/token", _domain);

        var resp = await httpClient.PostAsJsonAsync($"https://{_domain}/oauth/token", body);
        var respBody = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            logger.LogError("Auth0 get management token failed status={Status} body={Body}",
                (int)resp.StatusCode, Truncate(respBody, 500));
            throw new InvalidOperationException(
                $"Auth0 get management token failed: {(int)resp.StatusCode} {respBody}");
        }

        logger.LogInformation("Auth0 get management token response status={Status}", (int)resp.StatusCode);

        var token = System.Text.Json.JsonSerializer.Deserialize<Auth0TokenResponse>(respBody);
        return token!.AccessToken;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}

public class Auth0UserDto
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
}

public class Auth0TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}

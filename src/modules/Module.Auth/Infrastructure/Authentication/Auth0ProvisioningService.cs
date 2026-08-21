using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Common.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Infrastructure.Authentication;

public class Auth0ProvisioningService(
    HttpClient httpClient,
    IOptions<Auth0Settings> options,
    IMemoryCache cache) : IAuth0ProvisioningService
{
    private readonly Auth0Settings _settings = options.Value;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);
    private const string CacheKey = "auth0:m2m_token";

    public async Task<Result<string>> EnsureTestUserAsync(string email, string password)
    {
        var tokenResult = await GetManagementApiTokenAsync();
        if (!tokenResult.IsSuccess)
            return tokenResult.Error;
        var token = tokenResult.Value;

        using var searchRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://{_settings.Domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}");
        searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var searchResp = await httpClient.SendAsync(searchRequest);
        var searchBody = await searchResp.Content.ReadAsStringAsync();

        if (!searchResp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 search users-by-email failed: {(int)searchResp.StatusCode} {searchBody}");

        var existing = System.Text.Json.JsonSerializer.Deserialize<List<Auth0UserDto>>(searchBody);
        if (existing is { Count: > 0 })
            return existing[0].UserId;

        var body = new
        {
            email,
            password,
            connection = _settings.Connection,
            email_verified = true
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"https://{_settings.Domain}/api/v2/users")
        {
            Content = JsonContent.Create(body)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await httpClient.SendAsync(createRequest);
        var createBody = await createResp.Content.ReadAsStringAsync();

        if (!createResp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 create user failed: {(int)createResp.StatusCode} {createBody}");

        var created = System.Text.Json.JsonSerializer.Deserialize<Auth0UserDto>(createBody);
        if (created is null || string.IsNullOrWhiteSpace(created.UserId))
            return new Error(ErrorCode.InternalError, "Auth0 create user response was empty");

        return created.UserId;
    }

    public async Task<Result<string>> CreateInvitationUserAsync(string email)
    {
        var tokenResult = await GetManagementApiTokenAsync();
        if (!tokenResult.IsSuccess)
            return tokenResult.Error;
        var token = tokenResult.Value;

        using var searchRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://{_settings.Domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}");
        searchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var searchResp = await httpClient.SendAsync(searchRequest);
        var searchBody = await searchResp.Content.ReadAsStringAsync();

        if (!searchResp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 search users-by-email failed: {(int)searchResp.StatusCode} {searchBody}");

        var existing = System.Text.Json.JsonSerializer.Deserialize<List<Auth0UserDto>>(searchBody);
        if (existing is { Count: > 0 })
            return existing[0].UserId;

        var tempPassword = GenerateTempPassword();
        var body = new
        {
            email,
            password = tempPassword,
            connection = _settings.Connection,
            email_verified = false,
            app_metadata = new { needsInvitation = true }
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"https://{_settings.Domain}/api/v2/users")
        {
            Content = JsonContent.Create(body)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await httpClient.SendAsync(createRequest);
        var createBody = await createResp.Content.ReadAsStringAsync();

        if (!createResp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 create invitation user failed: {(int)createResp.StatusCode} {createBody}");

        var created = System.Text.Json.JsonSerializer.Deserialize<Auth0UserDto>(createBody);
        if (created is null || string.IsNullOrWhiteSpace(created.UserId))
            return new Error(ErrorCode.InternalError, "Auth0 create invitation user response was empty");

        return created.UserId;
    }

    public async Task<Result<bool>> SendInvitationAsync(string email)
    {
        var body = new
        {
            client_id = _settings.SpaClientId,
            email,
            connection = _settings.Connection
        };

        var resp = await httpClient.PostAsJsonAsync($"https://{_settings.Domain}/dbconnections/change_password", body);
        var respBody = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 send invitation failed: {(int)resp.StatusCode} {respBody}");

        return true;
    }

    public async Task<Result<string>> CreatePasswordChangeTicketAsync(string auth0UserId, string? resultUrl = null, int ttlSec = 432000)
    {
        var tokenResult = await GetManagementApiTokenAsync();
        if (!tokenResult.IsSuccess)
            return tokenResult.Error;
        var token = tokenResult.Value;

        var body = new
        {
            user_id = auth0UserId,
            result_url = resultUrl ?? $"https://{_settings.Domain}/login",
            ttl_sec = ttlSec,
            mark_email_as_verified = true,
            includeEmailInRedirect = false
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://{_settings.Domain}/api/v2/tickets/password-change")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await httpClient.SendAsync(request);
        var respBody = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return new Error(ErrorCode.InternalError,
                $"Auth0 create password-change ticket failed: {(int)resp.StatusCode} {respBody}");

        var ticket = System.Text.Json.JsonSerializer.Deserialize<PasswordChangeTicketResponse>(respBody);
        if (ticket is null || string.IsNullOrWhiteSpace(ticket.Ticket))
            return new Error(ErrorCode.InternalError, "Auth0 ticket response was empty");

        return ticket.Ticket;
    }

    private async Task<Result<string>> GetManagementApiTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_settings.M2M.StaticAccessToken))
            return _settings.M2M.StaticAccessToken!;

        if (cache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
            return cachedToken!;

        await _tokenLock.WaitAsync();
        try
        {
            if (cache.TryGetValue(CacheKey, out string? cachedAfterLock) && !string.IsNullOrWhiteSpace(cachedAfterLock))
                return cachedAfterLock!;

            var body = new
            {
                grant_type = "client_credentials",
                client_id = _settings.M2M.ClientId,
                client_secret = _settings.M2M.ClientSecret,
                audience = $"https://{_settings.Domain}/api/v2/"
            };

            var resp = await httpClient.PostAsJsonAsync($"https://{_settings.Domain}/oauth/token", body);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return new Error(ErrorCode.InternalError,
                    $"Auth0 get management token failed: {(int)resp.StatusCode} {respBody}");

            var token = System.Text.Json.JsonSerializer.Deserialize<Auth0TokenResponse>(respBody);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
                return new Error(ErrorCode.InternalError, "Auth0 token response was empty");

            var expiresIn = token.ExpiresIn > 60 ? token.ExpiresIn - 60 : token.ExpiresIn;
            if (expiresIn <= 0) expiresIn = 86400;

            cache.Set(CacheKey, token.AccessToken, TimeSpan.FromSeconds(expiresIn));

            return token.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static string GenerateTempPassword()
    {
        // 20 chars: Guid (32 hex) truncated + complexity suffix to satisfy Auth0 policy
        return $"Tmp_{Guid.NewGuid():N}{Guid.NewGuid():N}"[..16] + "Aa1!";
    }
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

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } = 86400;
}

public class PasswordChangeTicketResponse
{
    [JsonPropertyName("ticket")]
    public string Ticket { get; set; } = string.Empty;
}

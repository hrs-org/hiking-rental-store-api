using System.Net.Http.Json;
using System.Text.Json;
using HRS.API.Configuration;
using HRS.API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HRS.API.Services;

public class Auth0ManagementService : IAuth0ManagementService
{
    private readonly HttpClient _httpClient;
    private readonly Auth0Options _options;
    private readonly ILogger<Auth0ManagementService> _logger;

    private string? _accessToken;
    private DateTime _accessTokenExpiry = DateTime.MinValue;

    public Auth0ManagementService(
        HttpClient httpClient,
        IOptions<Auth0Options> options,
        ILogger<Auth0ManagementService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task AssignRoleAsync(string auth0UserId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(_options.Domain))
        {
            _logger.LogWarning("Auth0 Domain is empty. Skip assigning role '{RoleName}' for user {UserId}", roleName, auth0UserId);
            return;
        }

        var roleId = await GetRoleIdAsync(roleName);
        if (string.IsNullOrWhiteSpace(roleId))
            throw new InvalidOperationException($"Auth0 role '{roleName}' not found.");

        var token = await GetManagementTokenAsync();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://{_options.Domain}/api/v2/users/{Uri.EscapeDataString(auth0UserId)}/roles");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { roles = new[] { roleId } });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAppMetadataAsync(string auth0UserId, object metadata)
    {
        if (string.IsNullOrWhiteSpace(_options.Domain))
        {
            _logger.LogWarning("Auth0 Domain is empty. Skip app_metadata update for user {UserId}", auth0UserId);
            return;
        }

        var token = await GetManagementTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"https://{_options.Domain}/api/v2/users/{Uri.EscapeDataString(auth0UserId)}");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { app_metadata = metadata });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetRoleIdAsync(string roleName)
    {
        var token = await GetManagementTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://{_options.Domain}/api/v2/roles?name_filter={Uri.EscapeDataString(roleName)}");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var role = doc.RootElement
            .EnumerateArray()
            .FirstOrDefault(r => string.Equals(r.GetProperty("name").GetString(), roleName, StringComparison.Ordinal));

        return role.ValueKind == JsonValueKind.Undefined ? string.Empty : role.GetProperty("id").GetString() ?? string.Empty;
    }

    private async Task<string> GetManagementTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _accessTokenExpiry)
            return _accessToken;

        if (string.IsNullOrWhiteSpace(_options.ManagementClientId) ||
            string.IsNullOrWhiteSpace(_options.ManagementClientSecret) ||
            string.IsNullOrWhiteSpace(_options.Domain))
        {
            throw new InvalidOperationException("Auth0 Management API credentials are not configured.");
        }

        var request = new
        {
            client_id = _options.ManagementClientId,
            client_secret = _options.ManagementClientSecret,
            audience = $"https://{_options.Domain}/api/v2/",
            grant_type = "client_credentials"
        };

        var response = await _httpClient.PostAsJsonAsync($"https://{_options.Domain}/oauth/token", request);
        response.EnsureSuccessStatusCode();

        var tokenRaw = await response.Content.ReadAsStringAsync();
        using var tokenDoc = JsonDocument.Parse(tokenRaw);

        _accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
        var expiresIn = tokenDoc.RootElement.GetProperty("expires_in").GetInt32();

        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new InvalidOperationException("Cannot parse Auth0 access token response.");

        _accessTokenExpiry = DateTime.UtcNow.AddSeconds(Math.Max(60, expiresIn - 60));

        return _accessToken;
    }
}

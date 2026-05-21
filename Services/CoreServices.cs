using ShopApp.Mobile.DTOs;
using System.Text.Json;

namespace ShopApp.Mobile.Services;

// Token Storage
public interface ITokenStorageService
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
    Task RemoveUserDataAsync();
}

public class TokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";
    public Task SaveTokenAsync(string token) => SecureStorage.Default.SetAsync(TokenKey, token);
    public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);
    public Task RemoveTokenAsync() { SecureStorage.Default.Remove(TokenKey); return Task.CompletedTask; }
    public Task RemoveUserDataAsync()
    {
        SecureStorage.Default.Remove(UserKey);
        return Task.CompletedTask;
    }
}

// Auth State
public interface IAuthStateService
{
    bool IsLoggedIn { get; }
    string? Username { get; }
    string? Role { get; }
    List<string> Permissions { get; }
    int UserId { get; }
    void SetUser(AuthResponseDto authResponse, int userId);
    void ClearUser();
    bool HasPermission(string permission);
    bool IsAdmin { get; }

    Task InitializeAsync();
}

public class AuthStateService : IAuthStateService
{
    private readonly ITokenStorageService _tokenStorage;

    public AuthStateService(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    private const string UserKey = "auth_user";
    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public List<string> Permissions { get; private set; } = new();
    public int UserId { get; private set; }
    public bool IsAdmin => Role == "Admin";

    public void SetUser(AuthResponseDto authResponse, int userId)
    {
        IsLoggedIn = true;
        Username = authResponse.Username;
        Role = authResponse.Role;
        Permissions = authResponse.Permissions;
        UserId = userId;

        var json = JsonSerializer.Serialize(authResponse);
        SecureStorage.Default.SetAsync(UserKey, json);
    }

    public void ClearUser()
    {
        IsLoggedIn = false;
        Username = null;
        Role = null;
        Permissions = new();
        UserId = 0;
        SecureStorage.Default.Remove(UserKey);
    }
    public async Task InitializeAsync()
    {
        var json = await SecureStorage.Default.GetAsync(UserKey);

        if (string.IsNullOrWhiteSpace(json))
            return;

        var user = JsonSerializer.Deserialize<AuthResponseDto>(json);

        if (user == null)
            return;

        var token = await _tokenStorage.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return;

        IsLoggedIn = true;
        Username = user.Username;
        Role = user.Role;
        Permissions = user.Permissions;

        UserId = GetUserIdFromToken(token);
    }

    private static int GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c =>
                c.Type == "nameid" ||
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }
        catch { return 0; }
    }


    public bool HasPermission(string permission) =>
        IsAdmin || Permissions.Contains(permission);
}

// Base API Service
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    void SetToken(string? token);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            token != null
            ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token)
            : null;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        return await DeserializeAsync<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions),
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        return await DeserializeAsync<T>(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions),
            System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);
        return await DeserializeAsync<T>(response);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content)) return default;
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }
}
using System.Net.Http.Json;
using RacingApp.Shared.Dtos;

namespace RacingApp.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly LocalStorageService _storage;
    private readonly CustomAuthStateProvider _authState;

    public AuthService(HttpClient http, LocalStorageService storage, CustomAuthStateProvider authState)
    {
        _http = http;
        _storage = storage;
        _authState = authState;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode)
            return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null)
            return null;

        await _storage.SetAsync(CustomAuthStateProvider.TokenKey, auth.Token);
        _authState.NotifyUserLoggedIn();
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        if (!response.IsSuccessStatusCode)
            return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null)
            return null;

        await _storage.SetAsync(CustomAuthStateProvider.TokenKey, auth.Token);
        _authState.NotifyUserLoggedIn();
        return auth;
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveAsync(CustomAuthStateProvider.TokenKey);
        _authState.NotifyUserLoggedOut();
    }
}

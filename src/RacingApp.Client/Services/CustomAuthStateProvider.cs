using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace RacingApp.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public const string TokenKey = "authToken";

    private readonly LocalStorageService _storage;

    public CustomAuthStateProvider(LocalStorageService storage) => _storage = storage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _storage.GetAsync(TokenKey);
            if (string.IsNullOrWhiteSpace(token) || JwtUtils.IsExpired(token))
                return CreateAnonymous();

            var identity = new ClaimsIdentity(JwtUtils.ParseClaims(token), authenticationType: "jwt", nameType: "name", roleType: "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return CreateAnonymous();
        }
    }

    public void NotifyUserLoggedIn() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void NotifyUserLoggedOut()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private static AuthenticationState CreateAnonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}

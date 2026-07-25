using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Zone55.Management.Authentication;

public sealed class ManagementAuthenticationService(
    IHttpClientFactory httpClientFactory,
    IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private const string TokenStorageKey = "zone55.management.accessToken";

    public async Task<bool> LoginAsync(string username, string password)
    {
        var httpClient = httpClientFactory.CreateClient("ManagementAuthApi");
        using var response = await httpClient.PostAsJsonAsync(
            "api/learnkit/admin/auth/login",
            new { username, password });

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (string.IsNullOrWhiteSpace(result?.AccessToken))
        {
            return false;
        }

        await jsRuntime.InvokeVoidAsync("zone55Auth.setToken", TokenStorageKey, result.AccessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    public async Task LogoutAsync()
    {
        await jsRuntime.InvokeVoidAsync("zone55Auth.removeToken", TokenStorageKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await jsRuntime.InvokeAsync<string?>("zone55Auth.getToken", TokenStorageKey);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();
        var identity = TryCreateIdentity(token);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static ClaimsIdentity TryCreateIdentity(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ClaimsIdentity();
        }

        try
        {
            var payload = token.Split('.')[1];
            payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=')
                .Replace('-', '+')
                .Replace('_', '/');

            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = document.RootElement;

            if (root.TryGetProperty("exp", out var exp)
                && DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow)
            {
                return new ClaimsIdentity();
            }

            var name = root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out var claim)
                ? claim.GetString()
                : "admin";

            return new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, name ?? "admin")],
                authenticationType: "Bearer");
        }
        catch
        {
            return new ClaimsIdentity();
        }
    }

    private sealed record LoginResponse(string AccessToken);
}

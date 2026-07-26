using Microsoft.AspNetCore.Http;

namespace Backend55.Portal.Services;

public sealed class AppState
{
    public const string LightTheme = "morning-light";
    public const string DarkTheme = "midnight-dark";
    public const string ThemeCookieName = "Backend55.Theme";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppState(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        Theme = ReadInitialTheme();
    }

    public string Theme { get; private set; }
    public bool IsSidebarCollapsed { get; private set; }
    public bool IsMobileMenuOpen { get; private set; }

    public event Action? Changed;

    public void SetTheme(string theme)
    {
        var selectedTheme = NormalizeTheme(theme);

        if (Theme == selectedTheme)
        {
            return;
        }

        Theme = selectedTheme;
        Changed?.Invoke();
    }

    public void ToggleTheme() =>
        SetTheme(Theme == LightTheme ? DarkTheme : LightTheme);

    public void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
        Changed?.Invoke();
    }

    public void ToggleMobileMenu()
    {
        IsMobileMenuOpen = !IsMobileMenuOpen;
        Changed?.Invoke();
    }

    public void CloseMobileMenu()
    {
        if (!IsMobileMenuOpen)
        {
            return;
        }

        IsMobileMenuOpen = false;
        Changed?.Invoke();
    }

    private string ReadInitialTheme()
    {
        var themeCookie = _httpContextAccessor
            .HttpContext?
            .Request
            .Cookies[ThemeCookieName];

        return NormalizeTheme(themeCookie);
    }

    private static string NormalizeTheme(string? theme) =>
        theme == LightTheme ? LightTheme : DarkTheme;
}

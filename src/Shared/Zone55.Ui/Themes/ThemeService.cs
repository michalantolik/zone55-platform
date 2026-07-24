using Microsoft.JSInterop;

namespace Zone55.Ui.Themes;

public sealed class ThemeService(IJSRuntime javaScript)
{
    private const string ModulePath = "./_content/Zone55.Ui/js/theme.js";
    private IJSObjectReference? _module;

    public event Action? Changed;

    public string CurrentThemeKey { get; private set; } = ThemeCatalog.DefaultThemeKey;
    public ThemeDefinition CurrentTheme => ThemeCatalog.Get(CurrentThemeKey);

    public async Task InitializeAsync()
    {
        _module ??= await javaScript.InvokeAsync<IJSObjectReference>("import", ModulePath);
        var storedTheme = await _module.InvokeAsync<string?>("initializeTheme", ThemeCatalog.DefaultThemeKey);
        CurrentThemeKey = ThemeCatalog.Get(storedTheme).Key;
        Changed?.Invoke();
    }

    public async Task SetThemeAsync(string themeKey)
    {
        var theme = ThemeCatalog.Get(themeKey);
        _module ??= await javaScript.InvokeAsync<IJSObjectReference>("import", ModulePath);
        await _module.InvokeVoidAsync("setTheme", theme.Key);
        CurrentThemeKey = theme.Key;
        Changed?.Invoke();
    }
}

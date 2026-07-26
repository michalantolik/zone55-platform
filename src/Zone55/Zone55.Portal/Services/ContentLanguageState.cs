using Microsoft.JSInterop;

namespace Zone55.Portal.Services;

public sealed class ContentLanguageState(IJSRuntime javaScript)
{
    private const string StorageKey = "zone55.content-language";
    private static readonly string[] SupportedLanguages = ["pl", "en", "de"];

    public string Current { get; private set; } = "en";

    public IReadOnlyCollection<string> All => SupportedLanguages;

    public async Task InitializeAsync()
    {
        var stored = await javaScript.InvokeAsync<string?>(
            "localStorage.getItem",
            StorageKey);

        Current = SupportedLanguages.Contains(
            stored,
            StringComparer.OrdinalIgnoreCase)
                ? stored!.ToLowerInvariant()
                : "en";
    }

    public async Task SetAsync(string languageCode)
    {
        if (!SupportedLanguages.Contains(
                languageCode,
                StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Current = languageCode.ToLowerInvariant();

        await javaScript.InvokeVoidAsync(
            "localStorage.setItem",
            StorageKey,
            Current);
    }
}

using Microsoft.JSInterop;

namespace Zone55.Portal.Services;

public sealed class ClientCrashDiagnostics(IJSRuntime js)
{
    public async Task<string?> InitializeAsync(string apiBaseUrl, string source)
    {
        try
        {
            return await js.InvokeAsync<string?>("zone55CrashDiagnostics.initialize", apiBaseUrl, source,
                new { application = source, startedAtUtc = DateTimeOffset.UtcNow });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Crash diagnostics initialization failed: {ex}");
            return null;
        }
    }

    public ValueTask SetContextAsync(object context) =>
        js.InvokeVoidAsync("zone55CrashDiagnostics.setContext", context);

    public ValueTask RecordAsync(string eventName, string message, object? extra = null) =>
        js.InvokeVoidAsync("zone55CrashDiagnostics.record", eventName, message, extra);
}

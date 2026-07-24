using BlogPlatform.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlogPlatform.App;

public sealed class GlobalErrorBoundary : ErrorBoundary
{
    [Inject] public IPreviewDiagnosticsClient Diagnostics { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public ILogger<GlobalErrorBoundary> Logger { get; set; } = default!;

    protected override async Task OnErrorAsync(Exception exception)
    {
        Logger.LogCritical(exception, "Unhandled App component exception at {Uri}", Navigation.Uri);
        var text = $"Uri={Navigation.Uri}\n{exception}";
        if (text.Length > 12000) text = text[..12000] + " [truncated]";
        await Diagnostics.WriteAsync("APP", "app-global", "App.ComponentUnhandledException", 0, text);
    }
}

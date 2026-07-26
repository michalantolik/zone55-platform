using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Backend55.Management.Services;

namespace Backend55.Management;

public sealed class GlobalErrorBoundary : ErrorBoundary
{
    [Inject] public PreviewDiagnosticsClient Diagnostics { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public ILogger<GlobalErrorBoundary> Logger { get; set; } = default!;

    protected override async Task OnErrorAsync(Exception exception)
    {
        Logger.LogCritical(exception, "Unhandled Management component exception at {Uri}", Navigation.Uri);
        var text = $"Uri={Navigation.Uri}\n{exception}";
        if (text.Length > 12000) text = text[..12000] + " [truncated]";
        await Diagnostics.WriteAsync("management-global", "Management.ComponentUnhandledException", 0, text);
    }
}

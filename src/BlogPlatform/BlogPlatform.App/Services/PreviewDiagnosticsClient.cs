using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace BlogPlatform.App.Services;

public sealed class PreviewDiagnosticsClient : IPreviewDiagnosticsClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(2);
    private readonly HttpClient _httpClient;

    public PreviewDiagnosticsClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        Enabled = configuration.GetValue<bool>("Features:PreviewDiagnostics:Enabled");
    }

    public bool Enabled { get; }

    public async Task WriteAsync(string source, string sessionId, string eventName, int sequence, string message)
    {
        if (!Enabled)
        {
            return;
        }

        try
        {
            using var timeoutCancellation = new CancellationTokenSource(RequestTimeout);
            using var response = await _httpClient.PostAsJsonAsync(
                "api/preview-diagnostics",
                new PreviewDiagnosticEntry(source, sessionId, eventName, sequence, message),
                timeoutCancellation.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[LIVE_PREVIEW] Diagnostics endpoint failed. Status={(int)response.StatusCode}; Session={sessionId}; Event={eventName}; Sequence={sequence}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LIVE_PREVIEW] Diagnostics transport failed. Session={sessionId}; Event={eventName}; {ex.GetType().Name}: {ex.Message}");
        }
    }
}

public sealed record PreviewDiagnosticEntry(
    string Source,
    string SessionId,
    string Event,
    int Sequence,
    string Message);

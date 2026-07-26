using System.Net.Http.Json;

namespace Backend55.Management.Services;

public sealed class PreviewDiagnosticsClient
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private readonly HttpClient _httpClient;

    public PreviewDiagnosticsClient(IHttpClientFactory factory, IConfiguration configuration)
    {
        _httpClient = factory.CreateClient("ManagementApi");
        Enabled = configuration.GetValue<bool>("Features:PreviewDiagnostics:Enabled");
    }

    public bool Enabled { get; }

    public async Task WriteAsync(string sessionId, string eventName, int sequence, string message)
    {
        if (!Enabled) return;

        try
        {
            using var cts = new CancellationTokenSource(Timeout);
            using var response = await _httpClient.PostAsJsonAsync(
                "api/preview-diagnostics",
                new { Source = "MANAGEMENT", SessionId = sessionId, Event = eventName, Sequence = sequence, Message = message },
                cts.Token);

            _ = response.IsSuccessStatusCode;
        }
        catch
        {
            // Diagnostics transport is best-effort and must never affect the application.
        }
    }
}

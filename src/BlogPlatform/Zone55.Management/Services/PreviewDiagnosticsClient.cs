using System.Net.Http.Json;

namespace Zone55.Management.Services;

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

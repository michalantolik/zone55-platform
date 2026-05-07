using System.Net.Http.Json;

namespace BlogPlatform.App.Services;

public sealed class PreviewDiagnosticsClient : IPreviewDiagnosticsClient
{
    private readonly HttpClient _httpClient;

    public PreviewDiagnosticsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task WriteAsync(
        string source,
        string eventName,
        int sequence,
        string message)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/preview-diagnostics",
                new PreviewDiagnosticEntry(
                    source,
                    eventName,
                    sequence,
                    message));

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"PREVIEW DIAGNOSTICS FAILED: StatusCode={(int)response.StatusCode}; Event={eventName}; Sequence={sequence}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"PREVIEW DIAGNOSTICS FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

public sealed record PreviewDiagnosticEntry(
    string Source,
    string Event,
    int Sequence,
    string Message);

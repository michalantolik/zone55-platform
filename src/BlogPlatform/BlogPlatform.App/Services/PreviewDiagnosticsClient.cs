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
            await _httpClient.PostAsJsonAsync(
                "api/preview-diagnostics",
                new PreviewDiagnosticEntry(
                    source,
                    eventName,
                    sequence,
                    message));
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"PREVIEW DIAGNOSTICS FAILED: {ex}");

            throw;
        }
    }
}

public sealed record PreviewDiagnosticEntry(
    string Source,
    string Event,
    int Sequence,
    string Message);

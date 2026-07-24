using System.Net.Http.Json;

namespace BlogPlatform.App.Services;

public sealed class PreviewDiagnosticsClient : IPreviewDiagnosticsClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(2);
    private const int MaxMessageLength = 2500;

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
            using var timeoutCancellation = new CancellationTokenSource(RequestTimeout);
            using var response = await _httpClient.PostAsJsonAsync(
                "api/preview-diagnostics",
                new PreviewDiagnosticEntry(
                    Normalize(source, 40),
                    Normalize(eventName, 120),
                    sequence,
                    Normalize(message, MaxMessageLength)),
                timeoutCancellation.Token);

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

    private static string Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + " [truncated]";
    }
}

public sealed record PreviewDiagnosticEntry(
    string Source,
    string Event,
    int Sequence,
    string Message);

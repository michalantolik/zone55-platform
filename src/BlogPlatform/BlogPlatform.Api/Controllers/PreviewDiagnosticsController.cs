using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/preview-diagnostics")]
public sealed class PreviewDiagnosticsController : ControllerBase
{
    private readonly ILogger<PreviewDiagnosticsController> _logger;

    public PreviewDiagnosticsController(ILogger<PreviewDiagnosticsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(4096)]
    public IActionResult Write([FromBody] PreviewDiagnosticEntry entry)
    {
        var message = Normalize(entry.Message, 3000);

        _logger.LogInformation(
            "LIVE_PREVIEW_DIAG Source={Source}; Sequence={Sequence}; Event={Event}; Message={Message}",
            entry.Source,
            entry.Sequence,
            entry.Event,
            message);

        return Accepted();
    }

    private static string Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var normalized = value
            .Trim()
            .Replace("\r", " ")
            .Replace("\n", " ");

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + " [truncated]";
    }
}

public sealed class PreviewDiagnosticEntry
{
    public string Source { get; set; } = "unknown";

    public string Event { get; set; } = "unknown";

    public int Sequence { get; set; }

    public string? Message { get; set; }
}

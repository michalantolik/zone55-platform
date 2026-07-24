using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/preview-diagnostics")]
public sealed class PreviewDiagnosticsController : ControllerBase
{
    private readonly ILogger<PreviewDiagnosticsController> _logger;
    private readonly PreviewDiagnosticsOptions _options;

    public PreviewDiagnosticsController(
        ILogger<PreviewDiagnosticsController> logger,
        IOptions<PreviewDiagnosticsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [HttpPost]
    [RequestSizeLimit(8192)]
    public IActionResult Write([FromBody] PreviewDiagnosticEntry entry)
    {
        if (!_options.Enabled)
        {
            return NotFound();
        }

        var source = Normalize(entry.Source, 40);
        var sessionId = Normalize(entry.SessionId, 80);
        var eventName = Normalize(entry.Event, 120);
        var message = Normalize(entry.Message, _options.MaxMessageLength);

        using (LogContext.PushProperty("App", source))
        {
            _logger.LogInformation(
                "LIVE_PREVIEW_DIAG Session={SessionId}; Source={Source}; Sequence={Sequence}; Event={Event}; Message={Message}",
                sessionId,
                source,
                entry.Sequence,
                eventName,
                message);
        }

        return Accepted();
    }

    private static string Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var normalized = value.Trim().Replace("\r", " ").Replace("\n", " ");
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + " [truncated]";
    }
}

public sealed class PreviewDiagnosticsOptions
{
    public bool Enabled { get; set; }
    public int MaxMessageLength { get; set; } = 5000;
}

public sealed class PreviewDiagnosticEntry
{
    public string Source { get; set; } = "unknown";
    public string SessionId { get; set; } = "unknown";
    public string Event { get; set; } = "unknown";
    public int Sequence { get; set; }
    public string? Message { get; set; }
}

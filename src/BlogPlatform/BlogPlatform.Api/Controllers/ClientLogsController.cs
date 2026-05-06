using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/client-logs")]
[EnableRateLimiting("ClientLogs")]
public sealed class ClientLogsController : ControllerBase
{
    private readonly ILogger<ClientLogsController> _logger;
    private readonly ClientLoggingOptions _options;

    public ClientLogsController(
        ILogger<ClientLogsController> logger,
        IOptions<ClientLoggingOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [HttpPost]
    [RequestSizeLimit(4096)]
    public IActionResult WriteClientLog([FromBody] ClientLogEntry log)
    {
        if (!_options.Enabled)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(log.Message))
        {
            return BadRequest("Log message is required.");
        }

        var safeMessage = log.Message.Trim();

        if (safeMessage.Length > _options.MaxMessageLength)
        {
            safeMessage = safeMessage[.._options.MaxMessageLength] + " [truncated]";
        }

        using (LogContext.PushProperty("App", "APP"))
        {
            const string messageTemplate = "APP client log: {Message}";

            switch (log.Level?.ToUpperInvariant())
            {
                case "TRACE":
                    _logger.LogTrace(messageTemplate, safeMessage);
                    break;

                case "DEBUG":
                    _logger.LogDebug(messageTemplate, safeMessage);
                    break;

                case "WARNING":
                    _logger.LogWarning(messageTemplate, safeMessage);
                    break;

                case "ERROR":
                    _logger.LogError(messageTemplate, safeMessage);
                    break;

                case "CRITICAL":
                    _logger.LogCritical(messageTemplate, safeMessage);
                    break;

                default:
                    _logger.LogInformation(messageTemplate, safeMessage);
                    break;
            }
        }

        return Accepted();
    }
}

public sealed class ClientLoggingOptions
{
    public bool Enabled { get; set; }

    public int MaxMessageLength { get; set; } = 2000;
}

public sealed class ClientLogEntry
{
    public string? Level { get; set; }

    public string? Message { get; set; }
}

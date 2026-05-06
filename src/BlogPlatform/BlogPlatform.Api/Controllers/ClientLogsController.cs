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
    private static readonly HashSet<string> AllowedLevels =
    [
        "TRACE",
        "DEBUG",
        "INFORMATION",
        "WARNING",
        "ERROR",
        "CRITICAL"
    ];

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

        var safeMessage = NormalizeMessage(log.Message, _options.MaxMessageLength);
        var level = NormalizeLevel(log.Level);

        using (LogContext.PushProperty("App", "APP"))
        {
            const string messageTemplate = "APP client log: {Message}";

            switch (level)
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

    private static string NormalizeMessage(string message, int maxLength)
    {
        var safeMessage = message
            .Trim()
            .Replace("\r", " ")
            .Replace("\n", " ");

        if (safeMessage.Length > maxLength)
        {
            safeMessage = safeMessage[..maxLength] + " [truncated]";
        }

        return safeMessage;
    }

    private static string NormalizeLevel(string? level)
    {
        var normalizedLevel = level?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedLevel))
        {
            return "INFORMATION";
        }

        return AllowedLevels.Contains(normalizedLevel)
            ? normalizedLevel
            : "INFORMATION";
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

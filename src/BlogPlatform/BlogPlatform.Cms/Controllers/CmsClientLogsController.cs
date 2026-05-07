using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/cms-client-logs")]
public sealed class CmsClientLogsController : ControllerBase
{
    private readonly ILogger<CmsClientLogsController> _logger;

    public CmsClientLogsController(ILogger<CmsClientLogsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(4096)]
    public IActionResult WriteClientLog([FromBody] CmsClientLogEntry log)
    {
        if (string.IsNullOrWhiteSpace(log.Message))
        {
            return BadRequest("Log message is required.");
        }

        var message = Normalize(log.Message, 3000);
        var level = log.Level?.Trim().ToUpperInvariant();

        switch (level)
        {
            case "DEBUG":
                _logger.LogDebug("LIVE_PREVIEW CMS browser: {Message}", message);
                break;

            case "WARNING":
                _logger.LogWarning("LIVE_PREVIEW CMS browser: {Message}", message);
                break;

            case "ERROR":
                _logger.LogError("LIVE_PREVIEW CMS browser: {Message}", message);
                break;

            default:
                _logger.LogInformation("LIVE_PREVIEW CMS browser: {Message}", message);
                break;
        }

        return Accepted();
    }

    private static string Normalize(string value, int maxLength)
    {
        var normalized = value
            .Trim()
            .Replace("\r", " ")
            .Replace("\n", " ");

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + " [truncated]";
    }
}

public sealed class CmsClientLogEntry
{
    public string? Level { get; set; }

    public string? Message { get; set; }
}

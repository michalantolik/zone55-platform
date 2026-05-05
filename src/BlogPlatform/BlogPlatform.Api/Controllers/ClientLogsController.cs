using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/client-logs")]
public sealed class ClientLogsController : ControllerBase
{
    private readonly ILogger<ClientLogsController> _logger;

    public ClientLogsController(ILogger<ClientLogsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult WriteClientLog([FromBody] ClientLogEntry log)
    {
        using (LogContext.PushProperty("App", "APP"))
        {
            var message = "APP client log: {Message}";

            switch (log.Level?.ToUpperInvariant())
            {
                case "TRACE":
                    _logger.LogTrace(message, log.Message);
                    break;

                case "DEBUG":
                    _logger.LogDebug(message, log.Message);
                    break;

                case "WARNING":
                    _logger.LogWarning(message, log.Message);
                    break;

                case "ERROR":
                    _logger.LogError(message, log.Message);
                    break;

                case "CRITICAL":
                    _logger.LogCritical(message, log.Message);
                    break;

                default:
                    _logger.LogInformation(message, log.Message);
                    break;
            }
        }

        return Ok();
    }
}

public sealed class ClientLogEntry
{
    public string? Level { get; set; }

    public string? Message { get; set; }
}

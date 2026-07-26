using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Backend55.Portal.Services.Diagnostics;

/// <summary>
/// Logs the lifecycle of Blazor Server circuits so disconnected or terminated
/// interactive sessions can be diagnosed from the application logs.
/// </summary>
public sealed class LoggingCircuitHandler : CircuitHandler
{
    private readonly ILogger<LoggingCircuitHandler> _logger;

    public LoggingCircuitHandler(ILogger<LoggingCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Blazor circuit {CircuitId} opened.",
            circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Blazor circuit {CircuitId} connected.",
            circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Blazor circuit {CircuitId} disconnected.",
            circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Blazor circuit {CircuitId} closed.",
            circuit.Id);

        return Task.CompletedTask;
    }
}

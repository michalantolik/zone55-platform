using System.Net.Http.Json;

namespace Zone55.Management.Services;

public sealed class ApiClientLogger(HttpClient httpClient, string categoryName) : ILogger
{
    private const int MaxMessageLength = 12000;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null) return;
        _ = SendAsync(logLevel, message, exception);
    }

    private async Task SendAsync(LogLevel level, string message, Exception? exception)
    {
        try
        {
            var full = $"[{categoryName}] {message}" + (exception is null ? string.Empty : $"\nException={exception}");
            if (full.Length > MaxMessageLength) full = full[..MaxMessageLength] + " [truncated]";
            await httpClient.PostAsJsonAsync("api/client-logs", new ClientLogEntry(level.ToString(), full));
        }
        catch { }
    }
}

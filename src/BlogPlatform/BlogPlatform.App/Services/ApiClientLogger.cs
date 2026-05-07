using System.Net.Http.Json;

namespace BlogPlatform.App.Services;

public sealed class ApiClientLogger : ILogger
{
    private const int MaxMessageLength = 3000;

    private readonly HttpClient _httpClient;
    private readonly string _categoryName;

    public ApiClientLogger(
        HttpClient httpClient,
        string categoryName)
    {
        _httpClient = httpClient;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _ = SendLogAsync(logLevel, message, exception);
    }

    private async Task SendLogAsync(
        LogLevel logLevel,
        string message,
        Exception? exception)
    {
        try
        {
            var fullMessage = exception is null
                ? $"[{_categoryName}] {message}"
                : $"[{_categoryName}] {message}. Exception: {exception.GetType().Name}: {exception.Message}";

            if (fullMessage.Length > MaxMessageLength)
            {
                fullMessage = fullMessage[..MaxMessageLength] + " [truncated]";
            }

            await _httpClient.PostAsJsonAsync(
                "api/client-logs",
                new ClientLogEntry(logLevel.ToString(), fullMessage));
        }
        catch
        {
            // Never allow browser-side logging failures to break the app.
        }
    }
}

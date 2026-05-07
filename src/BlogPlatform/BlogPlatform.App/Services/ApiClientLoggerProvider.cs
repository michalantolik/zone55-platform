using Microsoft.Extensions.Logging;

namespace BlogPlatform.App.Services;

public sealed class ApiClientLoggerProvider : ILoggerProvider
{
    private readonly HttpClient _httpClient;

    public ApiClientLoggerProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName.Contains("ApiClientLogger"))
        {
            return new NullLogger();
        }

        if (categoryName.Contains("System.Net.Http"))
        {
            return new NullLogger();
        }

        if (categoryName.Contains("Microsoft.AspNetCore"))
        {
            return new NullLogger();
        }

        return new ApiClientLogger(_httpClient, categoryName);
    }

    public void Dispose()
    {
    }

    private sealed class NullLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}

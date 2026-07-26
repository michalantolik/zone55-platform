namespace Backend55.Management.Services;

public sealed class ApiClientLoggerProvider(HttpClient httpClient) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName.Contains(nameof(ApiClientLogger), StringComparison.Ordinal) ||
            categoryName.Contains("System.Net.Http", StringComparison.Ordinal))
            return NullLogger.Instance;
        return new ApiClientLogger(httpClient, categoryName);
    }
    public void Dispose() { }

    private sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}

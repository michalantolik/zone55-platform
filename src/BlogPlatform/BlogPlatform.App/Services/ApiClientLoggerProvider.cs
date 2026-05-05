namespace BlogPlatform.App.Services;

public sealed class ApiClientLoggerProvider : ILoggerProvider
{
    private readonly string _apiBaseUrl;

    public ApiClientLoggerProvider(string apiBaseUrl)
    {
        _apiBaseUrl = apiBaseUrl;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ApiClientLogger(_apiBaseUrl, categoryName);
    }

    public void Dispose()
    {
    }
}

namespace BlogPlatform.App.Services;

public sealed class ApiClientLoggerProvider : ILoggerProvider
{
    private readonly HttpClient _httpClient;

    public ApiClientLoggerProvider(string apiBaseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
        };
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ApiClientLogger(_httpClient, categoryName);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

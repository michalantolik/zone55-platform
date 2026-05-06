namespace BlogPlatform.Infrastructure.Cms;

public sealed class UmbracoDeliveryApiOptions
{
    public const string SectionName = "UmbracoDeliveryApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string PostsEndpoint { get; set; } = "api/blog-content/articles";

    public int FreshCacheSeconds { get; set; } = 600;

    public int StaleCacheSeconds { get; set; } = 3600;

    public int TimeoutSeconds { get; set; } = 10;
}

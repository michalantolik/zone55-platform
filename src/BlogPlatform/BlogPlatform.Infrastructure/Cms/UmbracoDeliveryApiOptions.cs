namespace BlogPlatform.Infrastructure.Cms;

public sealed class UmbracoDeliveryApiOptions
{
    public const string SectionName = "UmbracoDeliveryApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string PostsEndpoint { get; set; } =
        "umbraco/delivery/api/v2/content?filter=contentType:blogArticle";
}

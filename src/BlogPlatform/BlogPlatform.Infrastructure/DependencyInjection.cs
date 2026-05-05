using BlogPlatform.Application.Posts;
using BlogPlatform.Infrastructure.Cms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.Configure<UmbracoDeliveryApiOptions>(
            configuration.GetSection(UmbracoDeliveryApiOptions.SectionName));

        services.AddHttpClient<IBlogPostQueryService, UmbracoDeliveryApiBlogPostQueryService>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<UmbracoDeliveryApiOptions>>()
                    .Value;

                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    throw new InvalidOperationException(
                        "UmbracoDeliveryApi:BaseUrl is missing.");
                }

                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}

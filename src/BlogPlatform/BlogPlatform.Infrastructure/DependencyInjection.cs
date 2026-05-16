using BlogPlatform.Application.Posts;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Infrastructure.Cms;
using BlogPlatform.Infrastructure.Persistence;
using BlogPlatform.Infrastructure.Roadmap;
using Microsoft.EntityFrameworkCore;
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

        services
            .AddOptions<UmbracoDeliveryApiOptions>()
            .Bind(configuration.GetSection(UmbracoDeliveryApiOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl),
                "UmbracoDeliveryApi:BaseUrl is missing.")
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "UmbracoDeliveryApi:BaseUrl must be an absolute URL.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PostsEndpoint),
                "UmbracoDeliveryApi:PostsEndpoint is missing.")
            .Validate(options => options.FreshCacheSeconds > 0,
                "UmbracoDeliveryApi:FreshCacheSeconds must be greater than zero.")
            .Validate(options => options.StaleCacheSeconds >= options.FreshCacheSeconds,
                "UmbracoDeliveryApi:StaleCacheSeconds must be greater than or equal to FreshCacheSeconds.")
            .Validate(options => options.TimeoutSeconds > 0,
                "UmbracoDeliveryApi:TimeoutSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddHttpClient<IBlogPostRepository, UmbracoDeliveryApiBlogPostRepository>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<UmbracoDeliveryApiOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        return services;
    }

    public static IServiceCollection AddSqlServerRoadmapStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("umbracoDbDSN");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'umbracoDbDSN' is missing.");
        }

        services.AddDbContext<BlogPlatformDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IDotnetRoadmapStore, SqlDotnetRoadmapStore>();

        return services;
    }

    public static IServiceCollection AddFileSystemRoadmapStorage(
        this IServiceCollection services,
        string filePath)
    {
        services.Configure<FileSystemRoadmapStoreOptions>(options =>
        {
            options.FilePath = filePath;
        });

        services.AddSingleton<IDotnetRoadmapStore, FileSystemDotnetRoadmapStore>();

        return services;
    }
}

using BlogPlatform.Application;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.BlogContent;
using BlogPlatform.Cms.Roadmap;
using BlogPlatform.Infrastructure;

namespace BlogPlatform.Cms;

public static class DependencyInjection
{
    public static IServiceCollection AddBlogPlatformCmsServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddControllers();

        services.AddScoped<
            IRoadmapArticleAssignmentChecker,
            CmsRoadmapArticleAssignmentChecker>();

        services.AddScoped<
            IBlogContentAdminService,
            BlogContentAdminService>();

        services.AddApplication();

        services.AddInfrastructurePosts(configuration);
        services.AddSqlServerRoadmapStorage(configuration);

        return services;
    }

    public static Task EnsureCmsDatabaseCreatedAsync(
        this IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        return configuration.EnsureInfrastructureDatabaseCreatedAsync(cancellationToken);
    }

    public static Task InitializeCmsStorageAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        return services.InitializeBlogPlatformDatabaseAsync(cancellationToken);
    }
}

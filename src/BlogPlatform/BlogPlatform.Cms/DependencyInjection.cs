using BlogPlatform.Application;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.BlogContent;
using BlogPlatform.Cms.Health;
using BlogPlatform.Cms.Roadmap;
using BlogPlatform.Infrastructure;
using BlogPlatform.Infrastructure.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using LearnKit.Application;
using LearnKit.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Cms;

public static class DependencyInjection
{
    public static IServiceCollection AddBlogPlatformCmsServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddControllers();

        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("CMS process is alive."),
                tags: ["live"])
            .AddCheck<SqlServerConnectionHealthCheck>(
                "sql-server",
                tags: ["ready"])
            .AddCheck<UmbracoRuntimeHealthCheck>(
                "umbraco-runtime",
                tags: ["ready"]);

        services.AddScoped<
            IRoadmapArticleAssignmentChecker,
            CmsRoadmapArticleAssignmentChecker>();

        services.AddScoped<
            IBlogContentAdminService,
            BlogContentAdminService>();

        services.AddScoped<ILearnKitArticleAdminService, LearnKitArticleAdminService>();
        services.AddLearnKitApplication();
        services.AddLearnKitInfrastructure(options =>
            options.UseSqlServer(configuration.GetConnectionString("Zone55Connection")));

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

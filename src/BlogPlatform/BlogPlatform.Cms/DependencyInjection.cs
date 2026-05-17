using BlogPlatform.Application;
using BlogPlatform.Application.Roadmap;
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

        services.AddApplication();

        services.AddInfrastructurePosts(configuration);
        services.AddSqlServerRoadmapStorage(configuration);

        return services;
    }
}

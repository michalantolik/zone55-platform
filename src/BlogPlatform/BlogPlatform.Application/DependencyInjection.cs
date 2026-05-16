using BlogPlatform.Application.Posts;
using BlogPlatform.Application.Roadmap;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationPosts(
        this IServiceCollection services)
    {
        services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
        services.AddScoped<IBlogHomeContentQueryService, BlogHomeContentQueryService>();

        return services;
    }

    public static IServiceCollection AddApplicationRoadmapQueries(
        this IServiceCollection services)
    {
        services.AddScoped<IRoadmapQueryService, RoadmapQueryService>();

        return services;
    }

    public static IServiceCollection AddApplicationRoadmapCommands(
        this IServiceCollection services)
    {
        services.AddScoped<IRoadmapCommandService, RoadmapCommandService>();

        return services;
    }
}

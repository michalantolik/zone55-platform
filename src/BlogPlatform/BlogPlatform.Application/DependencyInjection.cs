using BlogPlatform.Application.Posts;
using BlogPlatform.Application.Roadmap;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
        services.AddScoped<IBlogHomeContentQueryService, BlogHomeContentQueryService>();

        services.AddScoped<IRoadmapQueryService, RoadmapQueryService>();
        services.AddScoped<IRoadmapCommandService, RoadmapCommandService>();
        services.AddScoped<IRoadmapSeedService, RoadmapSeedService>();

        return services;
    }
}

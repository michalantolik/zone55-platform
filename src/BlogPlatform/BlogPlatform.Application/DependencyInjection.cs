using BlogPlatform.Application.Posts;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
        services.AddScoped<IBlogHomeContentQueryService, BlogHomeContentQueryService>();

        return services;
    }
}

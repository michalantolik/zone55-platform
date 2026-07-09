using LearnKit.Application.Articles.Queries.GetArticleBySlug;
using LearnKit.Application.Roadmaps.Queries.GetLearningPath;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKit.Application;

/// <summary>
/// Registers LearnKit application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all LearnKit application services.
    /// </summary>
    public static IServiceCollection AddLearnKitApplication(
        this IServiceCollection services)
    {
        services.AddScoped<GetArticleBySlugHandler>();
        services.AddScoped<GetLearningPathHandler>();

        return services;
    }
}

using LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;
using LearnKit.Application.Articles.Admin.Queries.GetArticlesForManagement;
using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using LearnKit.Application.Roadmaps.Public.Queries.GetLearningPath;
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

        services.AddScoped<GetArticlesForManagementHandler>();
        services.AddScoped<GetArticleForEditingHandler>();

        return services;
    }
}

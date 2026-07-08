using LearnKit.Application.Articles.Queries.GetArticleBySlug;
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

        return services;
    }
}

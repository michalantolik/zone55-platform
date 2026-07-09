using LearnKit.Application.Articles.Contracts;
using LearnKit.Application.Roadmaps.Contracts;
using LearnKit.Infrastructure.Articles;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Roadmaps;
using LearnKit.Infrastructure.Seed;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKit.Infrastructure;

/// <summary>
/// Registers infrastructure services for LearnKit.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure implementations used by the application layer.
    /// </summary>
    public static IServiceCollection AddLearnKitInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<LearnKitDbContext>(configureDbContext);

        services.AddScoped<IArticleStore, EfArticleStore>();
        services.AddScoped<ILearningPathStore, EfLearningPathStore>();

        services.AddScoped<LearnKitContentSeedLoader>();
        services.AddScoped<LearnKitContentImporter>();
        services.AddScoped<LearnKitDatabaseSeeder>();

        return services;
    }
}
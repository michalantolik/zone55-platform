using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Public.Contracts;
using LearnKit.Application.Roadmaps.Public.Contracts;
using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Infrastructure.Articles;
using LearnKit.Infrastructure.Articles.Public;
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
        services.AddScoped<IArticleManagementStore, EfArticleManagementStore>();

        services.AddScoped<ILearningPathStore, EfLearningPathStore>();
        services.AddScoped<ILearningPathManagementStore, EfLearningPathManagementStore>();

        services.AddScoped<LearnKitContentSeedLoader>();
        services.AddScoped<LearnKitContentImporter>();
        services.AddScoped<LearnKitDatabaseSeeder>();
        services.AddScoped<LearnKitDatabaseInitializer>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        return services;
    }
}
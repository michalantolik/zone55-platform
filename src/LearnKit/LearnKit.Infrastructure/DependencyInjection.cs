using LearnKit.Application.Articles.Contracts;
using LearnKit.Infrastructure.Articles;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed;
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
        services.AddScoped<LearnKitDatabaseSeeder>();

        return services;
    }
}

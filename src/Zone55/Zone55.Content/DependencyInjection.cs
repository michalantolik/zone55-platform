using LearnKit.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace Zone55.Content;

/// <summary>
/// Registers the Zone55-specific LearnKit content.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Zone55 content seed source.
    /// </summary>
    public static IServiceCollection AddZone55Content(
        this IServiceCollection services)
    {
        services.AddSingleton<ILearnKitContentSeedSource, Zone55ContentSeedSource>();

        return services;
    }
}

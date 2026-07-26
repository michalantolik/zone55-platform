using LearnKit.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace Backend55.Content;

/// <summary>
/// Registers the Backend55-specific LearnKit content.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Backend55 content seed source.
    /// </summary>
    public static IServiceCollection AddBackend55Content(
        this IServiceCollection services)
    {
        services.AddSingleton<ILearnKitContentSeedSource, Backend55ContentSeedSource>();

        return services;
    }
}

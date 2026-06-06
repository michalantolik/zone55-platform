using Microsoft.Extensions.Diagnostics.HealthChecks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Health;

public sealed class UmbracoRuntimeHealthCheck(
    IRuntimeState runtimeState) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (runtimeState.Level == RuntimeLevel.Run)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("Umbraco runtime is running."));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy(
                $"Umbraco runtime is not ready. Runtime level: {runtimeState.Level}."));
    }
}

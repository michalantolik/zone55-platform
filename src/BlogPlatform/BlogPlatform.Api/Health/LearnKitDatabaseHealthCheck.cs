using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Api.Health;

/// <summary>
/// Verifies that the API can connect to the LearnKit database.
/// </summary>
public sealed class LearnKitDatabaseHealthCheck : IHealthCheck
{
    private readonly LearnKitDbContext _dbContext;

    public LearnKitDatabaseHealthCheck(LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database
                .CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("LearnKit database is available.")
                : HealthCheckResult.Unhealthy("LearnKit database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "LearnKit database health check failed.",
                exception);
        }
    }
}

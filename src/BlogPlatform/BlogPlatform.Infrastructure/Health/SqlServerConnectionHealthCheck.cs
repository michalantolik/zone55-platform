using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Infrastructure.Health;

public sealed class SqlServerConnectionHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqlServerConnectionHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("umbracoDbDSN");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy(
                "Connection string 'umbracoDbDSN' is missing.");
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5;

            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("SQL Server connection is ready.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "SQL Server connection is not ready.",
                ex);
        }
    }
}

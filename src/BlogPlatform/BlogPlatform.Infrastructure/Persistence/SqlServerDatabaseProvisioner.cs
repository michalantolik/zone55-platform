using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BlogPlatform.Infrastructure.Persistence;

public static class SqlServerDatabaseProvisioner
{
    public static async Task EnsureDatabaseCreatedAsync(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("umbracoDbDSN");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        builder.InitialCatalog = "master";
        builder.ConnectTimeout = 15;
        builder.Pooling = false;

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var escapedDatabaseName = databaseName.Replace("'", "''");
        var bracketedDatabaseName = databaseName.Replace("]", "]]");

        var commandText = $"""
            IF DB_ID(N'{escapedDatabaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{bracketedDatabaseName}]
            END
            """;

        await using var command = new SqlCommand(commandText, connection)
        {
            CommandTimeout = 120
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

using Microsoft.Data.SqlClient;

namespace BlogPlatform.Cms.Infrastructure.Database;

public static class SqlServerDatabaseInitializer
{
    public static async Task EnsureDatabaseCreatedAsync(IConfiguration configuration)
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

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var commandText = $"""
            IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL
            BEGIN
                CREATE DATABASE [{databaseName.Replace("]", "]]")}]
            END
            """;

        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }
}

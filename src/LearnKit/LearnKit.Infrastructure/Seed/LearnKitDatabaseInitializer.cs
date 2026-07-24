using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Applies database migrations and performs the one-time content bootstrap.
/// </summary>
public sealed class LearnKitDatabaseInitializer
{
    private readonly LearnKitDbContext _dbContext;
    private readonly LearnKitDatabaseSeeder _databaseSeeder;
    private readonly ILogger<LearnKitDatabaseInitializer> _logger;

    public LearnKitDatabaseInitializer(
        LearnKitDbContext dbContext,
        LearnKitDatabaseSeeder databaseSeeder,
        ILogger<LearnKitDatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _databaseSeeder = databaseSeeder;
        _logger = logger;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        var result = await _databaseSeeder.SeedAsync(cancellationToken);

        _logger.LogInformation(
            "LearnKit database initialization completed with result {InitializationResult}.",
            result);
    }
}

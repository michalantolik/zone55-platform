using LearnKit.Domain.Roadmaps;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Tests.Seed;

public sealed class LearnKitDatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldImportContentAndRecordInitialization_WhenDatabaseIsEmpty()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seeder = CreateSeeder(database.Context);

        var result = await seeder.SeedAsync();

        Assert.Equal(LearnKitInitializationResult.Seeded, result);
        Assert.True(await database.Context.LearningPaths.AnyAsync());
        Assert.Single(await database.Context.LearnKitInitializations.ToListAsync());
    }

    [Fact]
    public async Task SeedAsync_ShouldAdoptExistingContentWithoutImportingSeed()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.LearningPaths.Add(
            new LearningPath("custom", "Custom path", "Managed in Zone55"));
        await database.Context.SaveChangesAsync();
        var seeder = CreateSeeder(database.Context);

        var result = await seeder.SeedAsync();

        Assert.Equal(LearnKitInitializationResult.ExistingContentAdopted, result);
        Assert.Equal("custom", Assert.Single(await database.Context.LearningPaths.ToListAsync()).Key);
        Assert.Single(await database.Context.LearnKitInitializations.ToListAsync());
    }

    [Fact]
    public async Task SeedAsync_ShouldNotRestoreContentAfterInitializationWasRecorded()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seeder = CreateSeeder(database.Context);
        await seeder.SeedAsync();

        database.Context.LearningPaths.RemoveRange(database.Context.LearningPaths);
        await database.Context.SaveChangesAsync();

        var result = await seeder.SeedAsync();

        Assert.Equal(LearnKitInitializationResult.AlreadyInitialized, result);
        Assert.Empty(await database.Context.LearningPaths.ToListAsync());
    }

    [Fact]
    public async Task SeedAsync_ShouldPreserveManagedChangesOnSubsequentStartup()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seeder = CreateSeeder(database.Context);
        await seeder.SeedAsync();

        var path = await database.Context.LearningPaths.SingleAsync();
        path.Rename("Managed title");
        path.UpdateSummary("Managed summary");
        await database.Context.SaveChangesAsync();

        var result = await seeder.SeedAsync();

        Assert.Equal(LearnKitInitializationResult.AlreadyInitialized, result);
        Assert.Equal(
            "Managed title",
            (await database.Context.LearningPaths.SingleAsync()).Title);
    }

    private static LearnKitDatabaseSeeder CreateSeeder(LearnKitDbContext dbContext)
    {
        return new LearnKitDatabaseSeeder(
            dbContext,
            new LearnKitContentImporter(dbContext, new LearnKitContentSeedLoader()),
            new FixedTimeProvider());
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 24, 20, 0, 0, TimeSpan.Zero);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(
            SqliteConnection connection,
            LearnKitDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public LearnKitDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<LearnKitDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new LearnKitDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

using System.Data;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Applies the initial LearnKit content bootstrap exactly once.
/// </summary>
public sealed class LearnKitDatabaseSeeder
{
    internal const string InitializationKey = "initial-content";
    internal const string SourceVersion = "learnkit-content.seed.v1";

    private static readonly string SeedFilePath =
        Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "Content",
            "learnkit-content.seed.json");

    private readonly LearnKitDbContext _dbContext;
    private readonly LearnKitContentImporter _contentImporter;
    private readonly TimeProvider _timeProvider;

    public LearnKitDatabaseSeeder(
        LearnKitDbContext dbContext,
        LearnKitContentImporter contentImporter,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _contentImporter = contentImporter;
        _timeProvider = timeProvider;
    }

    public async Task<LearnKitInitializationResult> SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var isInitialized = await _dbContext.LearnKitInitializations
            .AnyAsync(
                record => record.Key == InitializationKey,
                cancellationToken);

        if (isInitialized)
        {
            await transaction.CommitAsync(cancellationToken);
            return LearnKitInitializationResult.AlreadyInitialized;
        }

        var hasExistingContent = await _dbContext.LearningPaths
            .AnyAsync(cancellationToken);

        if (!hasExistingContent)
        {
            await _contentImporter.ImportAsync(
                SeedFilePath,
                cancellationToken);
        }

        _dbContext.LearnKitInitializations.Add(
            new LearnKitInitializationRecord(
                InitializationKey,
                _timeProvider.GetUtcNow(),
                SourceVersion));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return hasExistingContent
            ? LearnKitInitializationResult.ExistingContentAdopted
            : LearnKitInitializationResult.Seeded;
    }
}

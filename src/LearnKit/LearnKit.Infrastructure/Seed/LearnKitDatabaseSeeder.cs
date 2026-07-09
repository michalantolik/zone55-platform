using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Seeds initial LearnKit data for local development.
/// </summary>
public sealed class LearnKitDatabaseSeeder
{
    private static readonly string SeedFilePath =
        Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "Content",
            "learnkit-content.seed.json");

    private readonly LearnKitDbContext _dbContext;
    private readonly LearnKitContentImporter _contentImporter;

    public LearnKitDatabaseSeeder(
        LearnKitDbContext dbContext,
        LearnKitContentImporter contentImporter)
    {
        _dbContext = dbContext;
        _contentImporter = contentImporter;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var hasContent = await _dbContext.LearningPaths
            .AnyAsync(cancellationToken);

        if (hasContent)
        {
            return;
        }

        await _contentImporter.ImportAsync(
            SeedFilePath,
            cancellationToken);
    }
}

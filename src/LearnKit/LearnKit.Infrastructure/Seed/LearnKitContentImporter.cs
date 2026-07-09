using LearnKit.Domain.Articles;
using LearnKit.Domain.Roadmaps;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed.Content.Models;
using System.Text.Json;

namespace LearnKit.Infrastructure.Seed.Content;

/// <summary>
/// Imports LearnKit content from a seed file into the database.
/// </summary>
public sealed class LearnKitContentImporter
{
    private readonly LearnKitDbContext _dbContext;
    private readonly LearnKitContentSeedLoader _seedLoader;

    public LearnKitContentImporter(
        LearnKitDbContext dbContext,
        LearnKitContentSeedLoader seedLoader)
    {
        _dbContext = dbContext;
        _seedLoader = seedLoader;
    }

    public async Task ImportAsync(
        string seedFilePath,
        CancellationToken cancellationToken = default)
    {
        var seed = await _seedLoader.LoadAsync(
            seedFilePath,
            cancellationToken);

        foreach (var pathSeed in seed.Content.LearningPaths)
        {
            var learningPath = CreateLearningPath(pathSeed);

            _dbContext.LearningPaths.Add(learningPath);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static LearningPath CreateLearningPath(
        LearningPathSeed seed)
    {
        var learningPath = new LearningPath(
            seed.Key,
            seed.Title,
            seed.Summary);

        foreach (var zoneSeed in seed.Zones)
        {
            learningPath.AddZone(CreateLearningZone(zoneSeed));
        }

        return learningPath;
    }

    private static LearningZone CreateLearningZone(
        LearningZoneSeed seed)
    {
        var learningZone = new LearningZone(
            seed.Key,
            seed.Title,
            seed.Summary,
            seed.SortOrder);

        foreach (var stepSeed in seed.Steps)
        {
            learningZone.AddStep(CreateLearningStep(stepSeed));
        }

        return learningZone;
    }

    private static LearningStep CreateLearningStep(
        LearningStepSeed seed)
    {
        var learningStep = new LearningStep(
            seed.Key,
            seed.Title,
            seed.Summary,
            seed.SortOrder);

        foreach (var articleSeed in seed.Articles)
        {
            learningStep.AddArticle(CreateArticle(learningStep.Id, articleSeed));
        }

        return learningStep;
    }

    private static Article CreateArticle(
        Guid learningStepId,
        ArticleSeed seed)
    {
        var article = new Article(
            learningStepId,
            seed.Slug,
            seed.Title,
            seed.Summary);

        if (seed.Status == ArticleStatus.Published)
        {
            article.Publish();
        }
        else if (seed.Status == ArticleStatus.Archived)
        {
            article.Archive();
        }

        foreach (var blockSeed in seed.Blocks)
        {
            article.AddBlock(CreateArticleBlock(blockSeed));
        }

        return article;
    }

    private static ArticleBlock CreateArticleBlock(
        ArticleBlockSeed seed)
    {
        return new ArticleBlock(
            seed.Type,
            seed.SortOrder,
            JsonSerializer.Serialize(seed.Content));
    }
}

using LearnKit.Domain.Articles;
using LearnKit.Domain.Articles.DomainModel;
using LearnKit.Domain.Articles.Entities;
using LearnKit.Domain.Articles.BusinessRules;
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

        await ImportAsync(seed, cancellationToken);
    }

    /// <summary>
    /// Imports LearnKit content from a seed stream into the database.
    /// </summary>
    public async Task ImportAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var seed = await _seedLoader.LoadAsync(
            stream,
            cancellationToken);

        await ImportAsync(seed, cancellationToken);
    }

    private async Task ImportAsync(
        LearnKitContentSeed seed,
        CancellationToken cancellationToken)
    {
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
            seed.Summary,
            seed.SortOrder);

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
        var translations = GetArticleTranslations(seed);
        var initial = SelectInitialTranslation(translations);

        var article = new Article(
            learningStepId,
            seed.Slug,
            initial.Value.Title,
            seed.SortOrder,
            initial.Value.Summary,
            initial.Key);

        foreach (var translation in translations)
        {
            article.SetTranslation(
                translation.Key,
                translation.Value.Title,
                translation.Value.Summary);

            ApplyStatus(
                article,
                translation.Key,
                translation.Value.Status);
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
        var translations = GetBlockTranslations(seed);
        var initial = SelectInitialTranslation(translations);

        var block = new ArticleBlock(
            seed.Type,
            seed.SortOrder,
            JsonSerializer.Serialize(initial.Value.Content),
            initial.Key);

        foreach (var translation in translations)
        {
            block.SetTranslation(
                translation.Key,
                JsonSerializer.Serialize(translation.Value.Content));
        }

        return block;
    }

    private static IReadOnlyDictionary<string, ArticleTranslationSeed>
        GetArticleTranslations(ArticleSeed seed)
    {
        if (seed.Translations.Count > 0)
        {
            return seed.Translations;
        }

        return new Dictionary<string, ArticleTranslationSeed>
        {
            [SupportedArticleLanguages.Default] = new()
            {
                Title = seed.Title,
                Summary = seed.Summary,
                Status = seed.Status
            }
        };
    }

    private static IReadOnlyDictionary<string, ArticleBlockTranslationSeed>
        GetBlockTranslations(ArticleBlockSeed seed)
    {
        if (seed.Translations.Count > 0)
        {
            return seed.Translations;
        }

        return new Dictionary<string, ArticleBlockTranslationSeed>
        {
            [SupportedArticleLanguages.Default] = new()
            {
                Content = seed.Content
            }
        };
    }

    private static KeyValuePair<string, T> SelectInitialTranslation<T>(
        IReadOnlyDictionary<string, T> translations)
    {
        return translations.TryGetValue(
            SupportedArticleLanguages.Default,
            out var defaultTranslation)
                ? new KeyValuePair<string, T>(
                    SupportedArticleLanguages.Default,
                    defaultTranslation)
                : translations
                    .OrderBy(item => item.Key, StringComparer.Ordinal)
                    .First();
    }

    private static void ApplyStatus(
        Article article,
        string languageCode,
        ArticleStatus status)
    {
        if (status == ArticleStatus.Published)
        {
            article.PublishTranslation(languageCode);

            if (string.Equals(
                    languageCode,
                    SupportedArticleLanguages.Default,
                    StringComparison.OrdinalIgnoreCase))
            {
                article.Publish();
            }
        }
        else if (status == ArticleStatus.Archived)
        {
            article.ArchiveTranslation(languageCode);

            if (string.Equals(
                    languageCode,
                    SupportedArticleLanguages.Default,
                    StringComparison.OrdinalIgnoreCase))
            {
                article.Archive();
            }
        }
    }
}

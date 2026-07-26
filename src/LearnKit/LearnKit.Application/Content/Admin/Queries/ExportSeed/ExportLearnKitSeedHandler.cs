using LearnKit.Application.Content.Admin.Contracts;
using LearnKit.Application.Content.Admin.Models;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Application.Content.Admin.Queries.ExportSeed;

public sealed class ExportLearnKitSeedHandler(
    ILearnKitContentPortabilityStore store)
{
    public async Task<LearnKitSeedExport> HandleAsync(
        ExportLearnKitSeedQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var export = await store.ExportAsync(cancellationToken);

        return new LearnKitSeedExport(
            export.SchemaVersion,
            new LearnKitSeedContent(
                export.Paths
                    .OrderBy(path => path.SortOrder)
                    .ThenBy(path => path.Key, StringComparer.Ordinal)
                    .Select(MapPath)
                    .ToList()));
    }

    private static LearningPathSeedExport MapPath(LearningPathExport path) =>
        new(
            path.Key,
            path.Title,
            path.Summary,
            path.SortOrder,
            path.Zones
                .OrderBy(zone => zone.SortOrder)
                .ThenBy(zone => zone.Key, StringComparer.Ordinal)
                .Select(MapZone)
                .ToList());

    private static LearningZoneSeedExport MapZone(LearningZoneExport zone) =>
        new(
            zone.Key,
            zone.Title,
            zone.Summary,
            zone.SortOrder,
            zone.Steps
                .OrderBy(step => step.SortOrder)
                .ThenBy(step => step.Key, StringComparer.Ordinal)
                .Select(MapStep)
                .ToList());

    private static LearningStepSeedExport MapStep(LearningStepExport step) =>
        new(
            step.Key,
            step.Title,
            step.Summary,
            step.SortOrder,
            step.Articles
                .OrderBy(article => article.SortOrder)
                .ThenBy(article => article.Slug, StringComparer.Ordinal)
                .Select(MapArticle)
                .ToList());

    private static ArticleSeedExport MapArticle(ArticleExport article) =>
        new(
            article.Slug,
            article.Title,
            article.Summary,
            (int)Enum.Parse<ArticleStatus>(
                article.Status,
                ignoreCase: true),
            article.SortOrder,
            article.Blocks
                .OrderBy(block => block.SortOrder)
                .ThenBy(block => block.Id)
                .Select(MapBlock)
                .ToList());

    private static ArticleBlockSeedExport MapBlock(ArticleBlockExport block) =>
        new(
            (int)Enum.Parse<ArticleBlockType>(
                block.Type,
                ignoreCase: true),
            block.SortOrder,
            block.Content);
}

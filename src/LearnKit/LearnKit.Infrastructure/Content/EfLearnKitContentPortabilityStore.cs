using System.Text.Json;
using LearnKit.Application.Content.Admin.Contracts;
using LearnKit.Application.Content.Admin.Models;
using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using LearnKit.Domain.Articles.Exceptions;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Content;

internal sealed class EfLearnKitContentPortabilityStore(
    LearnKitDbContext dbContext,
    TimeProvider timeProvider) : ILearnKitContentPortabilityStore
{
    public async Task<LearnKitContentExport> ExportAsync(CancellationToken cancellationToken = default)
    {
        var paths = await dbContext.LearningPaths
            .AsNoTracking()
            .Include(path => path.Zones)
                .ThenInclude(zone => zone.Steps)
                    .ThenInclude(step => step.Articles)
                        .ThenInclude(article => article.Blocks)
            .OrderBy(path => path.SortOrder)
            .ThenBy(path => path.Key)
            .ToListAsync(cancellationToken);

        return new LearnKitContentExport(
            1,
            timeProvider.GetUtcNow(),
            paths.Select(path => new LearningPathExport(
                path.Id,
                path.Key,
                path.Title,
                path.Summary,
                path.SortOrder,
                path.Zones.OrderBy(zone => zone.SortOrder).ThenBy(zone => zone.Key)
                    .Select(zone => new LearningZoneExport(
                        zone.Id,
                        zone.Key,
                        zone.Title,
                        zone.Summary,
                        zone.SortOrder,
                        zone.Steps.OrderBy(step => step.SortOrder).ThenBy(step => step.Key)
                            .Select(step => new LearningStepExport(
                                step.Id,
                                step.Key,
                                step.Title,
                                step.Summary,
                                step.SortOrder,
                                step.Articles.OrderBy(article => article.SortOrder).ThenBy(article => article.Slug)
                                    .Select(article => new ArticleExport(
                                        article.Id,
                                        article.Slug,
                                        article.Title,
                                        article.Summary,
                                        article.SortOrder,
                                        article.Status.ToString(),
                                        article.Blocks.OrderBy(block => block.SortOrder).ThenBy(block => block.Id)
                                            .Select(block => new ArticleBlockExport(
                                                block.Id,
                                                block.Type.ToString(),
                                                block.SortOrder,
                                                ParseContent(block.ContentJson)))
                                            .ToList()))
                                    .ToList()))
                            .ToList()))
                    .ToList()))
                .ToList());
    }

    public async Task<LearnKitContentValidationReport> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var export = await ExportAsync(cancellationToken);
        var issues = new List<LearnKitContentValidationIssue>();

        ValidateUnique(export.Paths, path => path.Key, "duplicate_path_key", "paths", issues);
        ValidateOrdered(export.Paths.SelectMany(path => path.Zones), zone => zone.SortOrder, zone => $"path/{FindPathKey(export, zone.Id)}/zone/{zone.Key}", "zone", issues);
        ValidateUnique(export.Paths.SelectMany(path => path.Zones), zone => zone.Key, "duplicate_zone_key", "zones", issues);
        ValidateUnique(export.Paths.SelectMany(path => path.Zones).SelectMany(zone => zone.Steps), step => step.Key, "duplicate_step_key", "steps", issues);
        ValidateUnique(export.Paths.SelectMany(path => path.Zones).SelectMany(zone => zone.Steps).SelectMany(step => step.Articles), article => article.Slug, "duplicate_article_slug", "articles", issues);

        foreach (var path in export.Paths)
        foreach (var zone in path.Zones)
        {
            ValidateSequence(zone.Steps.Select(step => step.SortOrder), $"path/{path.Key}/zone/{zone.Key}/steps", "step_order_invalid", issues);

            foreach (var step in zone.Steps)
            {
                ValidateSequence(step.Articles.Select(article => article.SortOrder), $"path/{path.Key}/zone/{zone.Key}/step/{step.Key}/articles", "article_order_invalid", issues);

                foreach (var article in step.Articles)
                {
                    ValidateSequence(article.Blocks.Select(block => block.SortOrder), $"article/{article.Slug}/blocks", "block_order_invalid", issues);

                    foreach (var block in article.Blocks)
                    {
                        try
                        {
                            ArticleBlockContentValidator.Validate(
                                Enum.Parse<ArticleBlockType>(block.Type),
                                block.Content.GetRawText());
                        }
                        catch (Exception exception) when (exception is ArgumentException or JsonException or InvalidArticleBlockException)
                        {
                            issues.Add(new("Error", "block_content_invalid", $"article/{article.Slug}/block/{block.Id}", exception.Message));
                        }
                    }
                }
            }
        }

        var zones = export.Paths.Sum(path => path.Zones.Count);
        var steps = export.Paths.SelectMany(path => path.Zones).Sum(zone => zone.Steps.Count);
        var articles = export.Paths.SelectMany(path => path.Zones).SelectMany(zone => zone.Steps).Sum(step => step.Articles.Count);
        var blocks = export.Paths.SelectMany(path => path.Zones).SelectMany(zone => zone.Steps).SelectMany(step => step.Articles).Sum(article => article.Blocks.Count);

        return new LearnKitContentValidationReport(
            issues.All(issue => !string.Equals(issue.Severity, "Error", StringComparison.OrdinalIgnoreCase)),
            timeProvider.GetUtcNow(),
            new(export.Paths.Count, zones, steps, articles, blocks),
            issues);
    }

    private static JsonElement ParseContent(string contentJson)
    {
        using var document = JsonDocument.Parse(contentJson);
        return document.RootElement.Clone();
    }

    private static void ValidateUnique<T>(IEnumerable<T> items, Func<T, string> keySelector, string code, string location, ICollection<LearnKitContentValidationIssue> issues)
    {
        foreach (var duplicate in items.GroupBy(keySelector, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            issues.Add(new("Error", code, location, $"The value '{duplicate.Key}' is used {duplicate.Count()} times."));
    }

    private static void ValidateOrdered<T>(IEnumerable<T> items, Func<T, int> orderSelector, Func<T, string> locationSelector, string itemName, ICollection<LearnKitContentValidationIssue> issues)
    {
        foreach (var item in items.Where(item => orderSelector(item) < 1))
            issues.Add(new("Error", $"{itemName}_order_invalid", locationSelector(item), "Sort order must be greater than zero."));
    }

    private static void ValidateSequence(IEnumerable<int> orders, string location, string code, ICollection<LearnKitContentValidationIssue> issues)
    {
        var actual = orders.OrderBy(value => value).ToArray();
        var expected = Enumerable.Range(1, actual.Length).ToArray();
        if (!actual.SequenceEqual(expected))
            issues.Add(new("Error", code, location, "Sort order must contain a continuous sequence starting at 1."));
    }

    private static string FindPathKey(LearnKitContentExport export, Guid zoneId) =>
        export.Paths.First(path => path.Zones.Any(zone => zone.Id == zoneId)).Key;
}

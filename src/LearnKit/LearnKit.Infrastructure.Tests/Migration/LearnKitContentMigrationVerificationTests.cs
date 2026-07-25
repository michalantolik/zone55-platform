using System.Text.Json;
using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using Xunit.Sdk;

namespace LearnKit.Infrastructure.Tests.Migration;

public sealed class LearnKitContentMigrationVerificationTests
{
    [Fact]
    public void LearnKitSeed_ShouldMatchLegacyUmbracoSeed()
    {
        using var legacyDocument = ReadSeed("Migration", "legacy-blog-content.seed.json");
        using var learnKitDocument = ReadSeed("Seed", "Content", "learnkit-content.seed.json");

        var report = Compare(legacyDocument.RootElement, learnKitDocument.RootElement);

        Assert.True(report.IsMatch, report.FormatFailure());
        Assert.Equal(4, report.ZoneCount);
        Assert.Equal(24, report.StepCount);
        Assert.Equal(109, report.ArticleCount);
        Assert.Equal(985, report.LegacyBlockCount);
        Assert.Equal(984, report.MigratedBlockCount);
        Assert.Equal(1, report.IgnoredEmptyBlockCount);
    }

    [Fact]
    public void MigratedBlocks_ShouldRemainRenderableByLearnKit()
    {
        using var document = ReadSeed("Seed", "Content", "learnkit-content.seed.json");
        var blocks = EnumerateLearnKitArticles(document.RootElement)
            .SelectMany(article => article.GetProperty("blocks").EnumerateArray())
            .ToArray();

        Assert.NotEmpty(blocks);

        foreach (var block in blocks)
        {
            var type = ResolveLearnKitType(block.GetProperty("type"));
            var contentJson = block.GetProperty("content").GetRawText();
            var exception = Record.Exception(() => ArticleBlockContentValidator.Validate(type, contentJson));

            Assert.Null(exception);
        }
    }

    private static MigrationVerificationReport Compare(JsonElement legacyRoot, JsonElement learnKitRoot)
    {
        var differences = new List<string>();
        var legacyZones = legacyRoot.GetProperty("roadmapZones").EnumerateArray().ToArray();
        var path = Assert.Single(learnKitRoot.GetProperty("content").GetProperty("learningPaths").EnumerateArray());
        var learnKitZones = path.GetProperty("zones").EnumerateArray().ToArray();

        CompareCollectionKeys(
            legacyZones,
            learnKitZones,
            "zone",
            element => element.GetProperty("key").GetString()!,
            differences);

        var stepCount = 0;
        foreach (var legacyZone in legacyZones)
        {
            var zoneKey = legacyZone.GetProperty("key").GetString()!;
            var learnKitZone = FindByKey(learnKitZones, zoneKey, "key", differences, "zone");
            if (learnKitZone is null)
            {
                continue;
            }

            CompareText(legacyZone, "name", learnKitZone.Value, "title", $"zone '{zoneKey}' title", differences);
            CompareNumber(legacyZone, "order", learnKitZone.Value, "sortOrder", $"zone '{zoneKey}' order", differences);

            var legacySteps = legacyZone.GetProperty("steps").EnumerateArray().ToArray();
            var learnKitSteps = learnKitZone.Value.GetProperty("steps").EnumerateArray().ToArray();
            stepCount += legacySteps.Length;

            CompareCollectionKeys(
                legacySteps,
                learnKitSteps,
                $"step in zone '{zoneKey}'",
                element => element.GetProperty("key").GetString()!,
                differences);

            foreach (var legacyStep in legacySteps)
            {
                var stepKey = legacyStep.GetProperty("key").GetString()!;
                var learnKitStep = FindByKey(learnKitSteps, stepKey, "key", differences, "step");
                if (learnKitStep is null)
                {
                    continue;
                }

                CompareText(legacyStep, "name", learnKitStep.Value, "title", $"step '{stepKey}' title", differences);
                CompareNumber(legacyStep, "order", learnKitStep.Value, "sortOrder", $"step '{stepKey}' order", differences);
            }
        }

        var legacyArticles = legacyRoot.GetProperty("articles").EnumerateArray().ToArray();
        var learnKitArticles = EnumerateLearnKitArticles(learnKitRoot).ToArray();
        CompareCollectionKeys(
            legacyArticles,
            learnKitArticles,
            "article",
            element => element.GetProperty("slug").GetString()!,
            differences);

        var blockCount = 0;
        var migratedBlockCount = 0;
        var ignoredEmptyBlockCount = 0;
        foreach (var legacyArticle in legacyArticles)
        {
            var slug = legacyArticle.GetProperty("slug").GetString()!;
            var learnKitArticle = FindByKey(learnKitArticles, slug, "slug", differences, "article");
            if (learnKitArticle is null)
            {
                continue;
            }

            CompareText(legacyArticle, "name", learnKitArticle.Value, "title", $"article '{slug}' title", differences);
            CompareText(legacyArticle, "summary", learnKitArticle.Value, "summary", $"article '{slug}' summary", differences);
            CompareNumber(legacyArticle, "order", learnKitArticle.Value, "sortOrder", $"article '{slug}' order", differences);
            CompareArticleLocation(legacyArticle, learnKitRoot, slug, differences);
            CompareArticleStatus(legacyArticle, learnKitArticle.Value, slug, differences);

            var allLegacyBlocks = legacyArticle.GetProperty("bodyBlocks").EnumerateArray().ToArray();
            var legacyBlocks = allLegacyBlocks.Where(block => !IsEmptyLegacyBlock(block)).ToArray();
            var learnKitBlocks = learnKitArticle.Value.GetProperty("blocks").EnumerateArray().ToArray();
            blockCount += allLegacyBlocks.Length;
            ignoredEmptyBlockCount += allLegacyBlocks.Length - legacyBlocks.Length;
            migratedBlockCount += learnKitBlocks.Length;

            if (legacyBlocks.Length != learnKitBlocks.Length)
            {
                differences.Add($"Article '{slug}' meaningful block count differs: legacy={legacyBlocks.Length}, LearnKit={learnKitBlocks.Length}.");
                continue;
            }

            for (var index = 0; index < legacyBlocks.Length; index++)
            {
                CompareBlock(legacyBlocks[index], learnKitBlocks[index], slug, index + 1, differences);
            }
        }

        return new MigrationVerificationReport(
            legacyZones.Length,
            stepCount,
            legacyArticles.Length,
            blockCount,
            migratedBlockCount,
            ignoredEmptyBlockCount,
            differences);
    }

    private static void CompareArticleLocation(
        JsonElement legacyArticle,
        JsonElement learnKitRoot,
        string slug,
        ICollection<string> differences)
    {
        var expectedZone = legacyArticle.GetProperty("dotnetZone").GetString();
        var expectedStep = legacyArticle.GetProperty("dotnetZoneStep").GetString();

        foreach (var path in learnKitRoot.GetProperty("content").GetProperty("learningPaths").EnumerateArray())
        foreach (var zone in path.GetProperty("zones").EnumerateArray())
        foreach (var step in zone.GetProperty("steps").EnumerateArray())
        foreach (var article in step.GetProperty("articles").EnumerateArray())
        {
            if (!string.Equals(article.GetProperty("slug").GetString(), slug, StringComparison.Ordinal))
            {
                continue;
            }

            var actualZone = zone.GetProperty("key").GetString();
            var actualStep = step.GetProperty("key").GetString();
            if (!string.Equals(expectedZone, actualZone, StringComparison.Ordinal) ||
                !string.Equals(expectedStep, actualStep, StringComparison.Ordinal))
            {
                differences.Add($"Article '{slug}' location differs: legacy={expectedZone}/{expectedStep}, LearnKit={actualZone}/{actualStep}.");
            }

            return;
        }
    }

    private static void CompareArticleStatus(
        JsonElement legacyArticle,
        JsonElement learnKitArticle,
        string slug,
        ICollection<string> differences)
    {
        var legacyLevel = legacyArticle.GetProperty("level").GetString();
        var expectedStatus = string.Equals(legacyLevel, "Published", StringComparison.OrdinalIgnoreCase)
            ? ArticleStatus.Published
            : ArticleStatus.Draft;
        var actualStatus = (ArticleStatus)learnKitArticle.GetProperty("status").GetInt32();

        if (expectedStatus != actualStatus)
        {
            differences.Add($"Article '{slug}' publication status differs: legacy={legacyLevel}, LearnKit={actualStatus}.");
        }
    }

    private static void CompareBlock(
        JsonElement legacyBlock,
        JsonElement learnKitBlock,
        string slug,
        int position,
        ICollection<string> differences)
    {
        var legacyType = legacyBlock.GetProperty("type").GetString()!;
        var expectedType = ResolveLegacyType(legacyType);
        var actualType = ResolveLearnKitType(learnKitBlock.GetProperty("type"));
        var actualOrder = learnKitBlock.GetProperty("sortOrder").GetInt32();

        if (expectedType != actualType)
        {
            differences.Add($"Article '{slug}' block {position} type differs: legacy={legacyType}, LearnKit={actualType}.");
        }

        if (actualOrder != position)
        {
            differences.Add($"Article '{slug}' block {position} has LearnKit sort order {actualOrder}.");
        }

        var content = learnKitBlock.GetProperty("content");
        foreach (var property in legacyBlock.EnumerateObject())
        {
            if (!content.TryGetProperty(property.Name, out var migratedValue))
            {
                differences.Add($"Article '{slug}' block {position} is missing content property '{property.Name}'.");
                continue;
            }

            if (!JsonElement.DeepEquals(property.Value, migratedValue))
            {
                differences.Add($"Article '{slug}' block {position} property '{property.Name}' differs.");
            }
        }
    }

    private static bool IsEmptyLegacyBlock(JsonElement block)
    {
        if (!string.Equals(block.GetProperty("type").GetString(), "text", StringComparison.Ordinal))
        {
            return false;
        }

        var text = block.TryGetProperty("text", out var textElement)
            ? textElement.GetString()
            : null;
        var rowsAreEmpty = !block.TryGetProperty("rows", out var rows) ||
                           rows.ValueKind == JsonValueKind.Array && rows.GetArrayLength() == 0;

        return string.IsNullOrWhiteSpace(text) && rowsAreEmpty;
    }

    private static ArticleBlockType ResolveLegacyType(string type) => type switch
    {
        "heading" or "text" => ArticleBlockType.Markdown,
        "codeSnippet" => ArticleBlockType.Code,
        "plantUmlDiagram" or "mermaidDiagram" => ArticleBlockType.Diagram,
        "table" => ArticleBlockType.Table,
        "callout" => ArticleBlockType.Callout,
        "summary" => ArticleBlockType.Summary,
        _ => throw new XunitException($"Unsupported legacy block type '{type}'.")
    };

    private static ArticleBlockType ResolveLearnKitType(JsonElement typeElement) => typeElement.ValueKind switch
    {
        JsonValueKind.Number when typeElement.TryGetInt32(out var numericType) &&
                                  Enum.IsDefined(typeof(ArticleBlockType), numericType) =>
            (ArticleBlockType)numericType,
        JsonValueKind.String when ArticleBlockTypeResolver.TryResolve(typeElement.GetString(), out var type) => type,
        _ => throw new XunitException($"Unsupported LearnKit block type {typeElement.GetRawText()}.")
    };

    private static IEnumerable<JsonElement> EnumerateLearnKitArticles(JsonElement root) =>
        root.GetProperty("content")
            .GetProperty("learningPaths")
            .EnumerateArray()
            .SelectMany(path => path.GetProperty("zones").EnumerateArray())
            .SelectMany(zone => zone.GetProperty("steps").EnumerateArray())
            .SelectMany(step => step.GetProperty("articles").EnumerateArray());

    private static JsonElement? FindByKey(
        IEnumerable<JsonElement> elements,
        string key,
        string propertyName,
        ICollection<string> differences,
        string entityName)
    {
        foreach (var element in elements)
        {
            if (string.Equals(element.GetProperty(propertyName).GetString(), key, StringComparison.Ordinal))
            {
                return element;
            }
        }

        differences.Add($"Missing LearnKit {entityName} '{key}'.");
        return null;
    }

    private static void CompareCollectionKeys(
        IReadOnlyCollection<JsonElement> legacy,
        IReadOnlyCollection<JsonElement> learnKit,
        string entityName,
        Func<JsonElement, string> keySelector,
        ICollection<string> differences)
    {
        var legacyValues = legacy.Select(keySelector).ToArray();
        var learnKitValues = learnKit.Select(keySelector).ToArray();
        var legacyKeys = legacyValues.ToHashSet(StringComparer.Ordinal);
        var learnKitKeys = learnKitValues.ToHashSet(StringComparer.Ordinal);

        foreach (var duplicate in legacyValues.GroupBy(value => value, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            differences.Add($"Duplicate legacy {entityName} '{duplicate.Key}'.");
        }

        foreach (var duplicate in learnKitValues.GroupBy(value => value, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            differences.Add($"Duplicate LearnKit {entityName} '{duplicate.Key}'.");
        }

        foreach (var key in legacyKeys.Except(learnKitKeys, StringComparer.Ordinal))
        {
            differences.Add($"Missing LearnKit {entityName} '{key}'.");
        }

        foreach (var key in learnKitKeys.Except(legacyKeys, StringComparer.Ordinal))
        {
            differences.Add($"Unexpected LearnKit {entityName} '{key}'.");
        }
    }

    private static void CompareText(
        JsonElement left,
        string leftProperty,
        JsonElement right,
        string rightProperty,
        string description,
        ICollection<string> differences)
    {
        var expected = left.GetProperty(leftProperty).GetString() ?? string.Empty;
        var actual = right.GetProperty(rightProperty).GetString() ?? string.Empty;
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            differences.Add($"{description} differs.");
        }
    }

    private static void CompareNumber(
        JsonElement left,
        string leftProperty,
        JsonElement right,
        string rightProperty,
        string description,
        ICollection<string> differences)
    {
        var expected = left.GetProperty(leftProperty).GetInt32();
        var actual = right.GetProperty(rightProperty).GetInt32();
        if (expected != actual)
        {
            differences.Add($"{description} differs: legacy={expected}, LearnKit={actual}.");
        }
    }

    private static JsonDocument ReadSeed(params string[] pathParts)
    {
        var path = Path.Combine(new[] { AppContext.BaseDirectory }.Concat(pathParts).ToArray());
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private sealed record MigrationVerificationReport(
        int ZoneCount,
        int StepCount,
        int ArticleCount,
        int LegacyBlockCount,
        int MigratedBlockCount,
        int IgnoredEmptyBlockCount,
        IReadOnlyCollection<string> Differences)
    {
        public bool IsMatch => Differences.Count == 0;

        public string FormatFailure() =>
            IsMatch
                ? string.Empty
                : "LearnKit migration verification failed:" + Environment.NewLine +
                  string.Join(Environment.NewLine, Differences.Select(difference => $"- {difference}"));
    }
}

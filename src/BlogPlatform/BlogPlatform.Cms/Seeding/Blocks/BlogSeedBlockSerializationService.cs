using System.Text.Json;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Seeding.Blocks;

public sealed class BlogSeedBlockSerializationService
{
    private readonly IReadOnlyCollection<ISeedBlockSerializationStrategy> _strategies;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<BlogSeedBlockSerializationService> _logger;

    public BlogSeedBlockSerializationService(
        IEnumerable<ISeedBlockSerializationStrategy> strategies,
        IContentTypeService contentTypeService,
        ILogger<BlogSeedBlockSerializationService> logger)
    {
        _strategies = strategies.ToList();
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    public BlogSeedBlock? ExportFromBlockListContent(JsonElement blockElement)
    {
        var elementTypeAlias =
            GetElementTypeAliasFromEditorType(blockElement) ??
            InferElementTypeAlias(blockElement) ??
            GetElementTypeAlias(blockElement);

        if (string.IsNullOrWhiteSpace(elementTypeAlias))
        {
            return null;
        }

        var strategy = _strategies.FirstOrDefault(strategy =>
            strategy.CanExport(elementTypeAlias, blockElement));

        return strategy?.Export(blockElement);
    }

    public SeedBlock ImportToSeedBlock(BlogSeedBlock block)
    {
        var strategy = _strategies.FirstOrDefault(strategy =>
            strategy.CanImport(block));

        if (strategy is null)
        {
            _logger.LogWarning(
                "Unknown seed block type '{BlockType}'. Falling back to text block.",
                block.Type);

            return new SeedBlock(
                BlogContentAliases.TextBlock,
                new Dictionary<string, object?>
                {
                    ["text"] = block.Text ?? string.Empty
                });
        }

        return strategy.Import(block);
    }

    public string NormalizeBodyBlocksForUmbraco(string? bodyBlocksJson)
    {
        if (string.IsNullOrWhiteSpace(bodyBlocksJson))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(bodyBlocksJson);

            var root = document.RootElement;

            if (!root.TryGetProperty("contentData", out var contentData) ||
                contentData.ValueKind != JsonValueKind.Array)
            {
                return bodyBlocksJson;
            }

            var normalizedContentData = new List<Dictionary<string, object?>>();
            var layoutItems = new List<Dictionary<string, object?>>();

            foreach (var item in contentData.EnumerateArray())
            {
                var normalized =
                    JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        item.GetRawText()) ?? [];

                var alias =
                    GetElementTypeAliasFromEditorType(item) ??
                    InferElementTypeAlias(item) ??
                    GetElementTypeAlias(item);

                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                var elementType = _contentTypeService.Get(alias);

                if (elementType is null)
                {
                    continue;
                }

                var udi = SeedBlockJson.GetString(item, "udi");

                if (string.IsNullOrWhiteSpace(udi))
                {
                    udi = $"umb://element/{Guid.NewGuid():N}";
                }

                normalized["udi"] = udi;
                normalized["contentTypeKey"] = elementType.Key;

                var strategy = _strategies.FirstOrDefault(strategy =>
                    string.Equals(
                        strategy.ElementTypeAlias,
                        alias,
                        StringComparison.OrdinalIgnoreCase));

                strategy?.NormalizeForUmbraco(normalized);

                normalizedContentData.Add(normalized);

                layoutItems.Add(new Dictionary<string, object?>
                {
                    ["contentUdi"] = udi,
                    ["settingsUdi"] = null
                });
            }

            return JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["layout"] = new Dictionary<string, object?>
                {
                    ["Umbraco.BlockList"] = layoutItems
                },
                ["contentData"] = normalizedContentData,
                ["settingsData"] = Array.Empty<object>()
            });
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not normalize article body blocks before saving.");

            return bodyBlocksJson;
        }
    }

    public string? ResolveElementTypeAlias(JsonElement blockElement)
    {
        return
            GetElementTypeAliasFromEditorType(blockElement) ??
            InferElementTypeAlias(blockElement) ??
            GetElementTypeAlias(blockElement);
    }

    private string? GetElementTypeAlias(JsonElement blockElement)
    {
        var contentTypeKeyValue =
            SeedBlockJson.GetString(blockElement, "contentTypeKey");

        if (string.IsNullOrWhiteSpace(contentTypeKeyValue) ||
            !Guid.TryParse(contentTypeKeyValue, out var contentTypeKey))
        {
            return null;
        }

        return _contentTypeService.Get(contentTypeKey)?.Alias;
    }

    private static string? GetElementTypeAliasFromEditorType(JsonElement blockElement)
    {
        var type = SeedBlockJson.GetString(blockElement, "type");

        return type?.Trim().ToLowerInvariant() switch
        {
            "text" => BlogContentAliases.TextBlock,

            "heading" => BlogContentAliases.HeadingBlock,

            "code" => BlogContentAliases.CodeSnippetBlock,
            "codesnippet" => BlogContentAliases.CodeSnippetBlock,

            "mermaid" => BlogContentAliases.MermaidDiagramBlock,
            "mermaiddiagram" => BlogContentAliases.MermaidDiagramBlock,

            "plantuml" => BlogContentAliases.PlantUmlDiagramBlock,
            "plantumldiagram" => BlogContentAliases.PlantUmlDiagramBlock,
            "plantumldiagramblock" => BlogContentAliases.PlantUmlDiagramBlock,

            "callout" => BlogContentAliases.CalloutBlock,

            "summary" => BlogContentAliases.SummaryBlock,

            "table" => BlogContentAliases.TableBlock,
            "tableblock" => BlogContentAliases.TableBlock,

            _ => null
        };
    }

    private static string? InferElementTypeAlias(JsonElement blockElement)
    {
        if (blockElement.TryGetProperty("summary", out _))
        {
            return BlogContentAliases.SummaryBlock;
        }

        if (blockElement.TryGetProperty("plantUml", out _) ||
            blockElement.TryGetProperty("plantuml", out _))
        {
            return BlogContentAliases.PlantUmlDiagramBlock;
        }

        if (blockElement.TryGetProperty("mermaid", out _))
        {
            return BlogContentAliases.MermaidDiagramBlock;
        }

        if (blockElement.TryGetProperty("diagram", out var diagram))
        {
            var text = diagram.GetString() ?? string.Empty;

            return text.TrimStart().StartsWith(
                       "@startuml",
                       StringComparison.OrdinalIgnoreCase)
                || text.TrimStart().StartsWith(
                       "@@startuml",
                       StringComparison.OrdinalIgnoreCase)
                ? BlogContentAliases.PlantUmlDiagramBlock
                : BlogContentAliases.MermaidDiagramBlock;
        }

        if (blockElement.TryGetProperty("code", out _) ||
            blockElement.TryGetProperty("language", out _) ||
            blockElement.TryGetProperty("fileName", out _))
        {
            return BlogContentAliases.CodeSnippetBlock;
        }

        if (blockElement.TryGetProperty("kind", out _) ||
            blockElement.TryGetProperty("calloutType", out _))
        {
            return BlogContentAliases.CalloutBlock;
        }

        if (blockElement.TryGetProperty("level", out _))
        {
            return BlogContentAliases.HeadingBlock;
        }

        if (blockElement.TryGetProperty("rows", out _))
        {
            return BlogContentAliases.TableBlock;
        }

        if (blockElement.TryGetProperty("text", out _))
        {
            return BlogContentAliases.TextBlock;
        }

        return null;
    }
}

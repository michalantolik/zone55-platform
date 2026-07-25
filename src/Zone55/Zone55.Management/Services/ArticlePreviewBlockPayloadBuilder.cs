using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using System.Text.Json;

namespace Zone55.Management.Services;

public static class ArticlePreviewBlockPayloadBuilder
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public static string Build(IEnumerable<ArticlePreviewBlockPayload> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var payload = new List<object>();

        foreach (var block in blocks.OrderBy(item => item.SortOrder))
        {
            if (!ArticleBlockTypeResolver.TryResolve(block.Type, out var blockType))
            {
                payload.Add(CreateUnsupportedBlock(block));
                continue;
            }

            if (!TryParseContent(block.ContentJson, out var content))
            {
                payload.Add(CreateInvalidJsonBlock(block, blockType));
                continue;
            }

            payload.Add(new
            {
                id = block.Id,
                type = blockType.ToString(),
                sortOrder = block.SortOrder,
                contentJson = content
            });
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static bool TryParseContent(string? contentJson, out JsonElement content)
    {
        content = default;

        if (string.IsNullOrWhiteSpace(contentJson))
        {
            content = JsonSerializer.SerializeToElement(new { }, JsonOptions);
            return true;
        }

        try
        {
            using var document = JsonDocument.Parse(contentJson);
            content = document.RootElement.Clone();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static object CreateUnsupportedBlock(ArticlePreviewBlockPayload block) => new
    {
        id = block.Id,
        type = ArticleBlockType.Markdown.ToString(),
        sortOrder = block.SortOrder,
        contentJson = new
        {
            markdown = $"Preview is unavailable for unsupported block type `{block.Type}`."
        }
    };

    private static object CreateInvalidJsonBlock(
        ArticlePreviewBlockPayload block,
        ArticleBlockType blockType) => new
    {
        id = block.Id,
        type = ArticleBlockType.Markdown.ToString(),
        sortOrder = block.SortOrder,
        contentJson = new
        {
            markdown = $"Preview is unavailable while the {blockType} block contains invalid JSON."
        }
    };
}

public sealed record ArticlePreviewBlockPayload(
    string Id,
    string Type,
    string ContentJson,
    int SortOrder);

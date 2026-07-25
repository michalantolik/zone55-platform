using Zone55.Portal.Models.LearnKit;
using Zone55.Portal.Models.LearnKit.Articles;
using System.Text.Json;
using LearnKit.Domain.Articles.BusinessRules;

namespace Zone55.Portal.Components.Articles.LearnKitRendering.Serialization;

/// <summary>
/// Converts a Zone55 Management live-preview payload into the article model rendered by the Portal.
/// </summary>
public static class LearnKitArticlePreviewMapper
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public static LearnKitArticleDetails ToArticle(
        LearnKitArticlePreviewPayload preview)
    {
        return new LearnKitArticleDetails
        {
            Id = $"preview-{preview.Slug}",
            Slug = preview.Slug,
            Title = preview.Title,
            Summary = preview.Summary,
            Status = "Preview",
            Blocks = ParseBlocks(preview.BodyContent)
        };
    }

    private static IReadOnlyList<LearnKitArticleBlockDetails> ParseBlocks(
        string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(body);

            return document.RootElement.ValueKind == JsonValueKind.Array
                ? ParseLearnKitBlocks(document.RootElement)
                : [];
        }
        catch (JsonException)
        {
            return
            [
                new LearnKitArticleBlockDetails
                {
                    Id = "preview-block-1",
                    Type = LearnKitBlockTypes.Markdown,
                    SortOrder = 1,
                    ContentJson = JsonSerializer.Serialize(
                        new
                        {
                            markdown = body
                        },
                        JsonOptions)
                }
            ];
        }
    }

    private static IReadOnlyList<LearnKitArticleBlockDetails> ParseLearnKitBlocks(
        JsonElement root)
    {
        var blocks = new List<LearnKitArticleBlockDetails>();
        var index = 0;

        foreach (var element in root.EnumerateArray())
        {
            index++;

            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty("type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var type = typeElement.GetString();

            if (!ArticleBlockTypeResolver.TryResolve(type, out var blockType))
            {
                blocks.Add(CreateUnsupportedBlock(element, index, type));
                continue;
            }

            var sortOrder = GetSortOrder(element, index);
            var id = GetBlockId(element, index);

            blocks.Add(new LearnKitArticleBlockDetails
            {
                Id = id,
                Type = blockType.ToString(),
                SortOrder = sortOrder,
                ContentJson = BuildContentJson(element)
            });
        }

        return blocks
            .OrderBy(block => block.SortOrder)
            .ThenBy(block => block.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildContentJson(JsonElement element)
    {
        if (element.TryGetProperty("contentJson", out var nestedContent))
        {
            if (nestedContent.ValueKind == JsonValueKind.String)
            {
                var serializedContent = nestedContent.GetString();

                if (!string.IsNullOrWhiteSpace(serializedContent))
                {
                    try
                    {
                        using var document = JsonDocument.Parse(serializedContent);
                        return document.RootElement.GetRawText();
                    }
                    catch (JsonException)
                    {
                        return JsonSerializer.Serialize(
                            new { markdown = serializedContent },
                            JsonOptions);
                    }
                }
            }

            if (nestedContent.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            {
                return nestedContent.GetRawText();
            }
        }

        var content =
            new Dictionary<string, JsonElement>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals("id") ||
                property.NameEquals("type") ||
                property.NameEquals("sortOrder") ||
                property.NameEquals("contentJson"))
            {
                continue;
            }

            content[property.Name] = property.Value.Clone();
        }

        return JsonSerializer.Serialize(content, JsonOptions);
    }

    private static int GetSortOrder(JsonElement element, int fallback) =>
        element.TryGetProperty("sortOrder", out var sortOrderElement) &&
        sortOrderElement.TryGetInt32(out var parsedSortOrder)
            ? parsedSortOrder
            : fallback;

    private static string GetBlockId(JsonElement element, int fallback) =>
        element.TryGetProperty("id", out var idElement) &&
        idElement.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(idElement.GetString())
            ? idElement.GetString()!
            : $"preview-block-{fallback}";

    private static LearnKitArticleBlockDetails CreateUnsupportedBlock(
        JsonElement element,
        int index,
        string? sourceType)
    {
        return new LearnKitArticleBlockDetails
        {
            Id = GetBlockId(element, index),
            Type = LearnKitBlockTypes.Markdown,
            SortOrder = GetSortOrder(element, index),
            ContentJson = JsonSerializer.Serialize(
                new
                {
                    markdown = $"Preview is unavailable for unsupported block type `{sourceType ?? "unknown"}`."
                },
                JsonOptions)
        };
    }

}

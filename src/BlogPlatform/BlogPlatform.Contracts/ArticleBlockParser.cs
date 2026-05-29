using BlogPlatform.Contracts.Posts.ArticleBlocks;
using System.Text.Json;

namespace BlogPlatform.Contracts.Posts;

public static class ArticleBlockParser
{
    public static IReadOnlyCollection<ArticleBlockDto> Parse(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return [];
        }

        using var document = JsonDocument.Parse(rawJson);

        var root = document.RootElement;

        if (!root.TryGetProperty("layout", out var layout) ||
            !layout.TryGetProperty("Umbraco.BlockList", out var layoutItems) ||
            !root.TryGetProperty("contentData", out var contentData))
        {
            return [];
        }

        var contentDataByUdi = contentData
            .EnumerateArray()
            .Where(x => x.TryGetProperty("udi", out _))
            .ToDictionary(
                x => x.GetProperty("udi").GetString() ?? string.Empty,
                x => x);

        var blocks = new List<ArticleBlockDto>();

        foreach (var layoutItem in layoutItems.EnumerateArray())
        {
            var contentUdi = layoutItem.GetProperty("contentUdi").GetString();

            if (string.IsNullOrWhiteSpace(contentUdi) ||
                !contentDataByUdi.TryGetValue(contentUdi, out var block))
            {
                continue;
            }

            blocks.Add(ArticleBlockDefinitionRegistry.Parse(block));
        }

        return blocks;
    }
}

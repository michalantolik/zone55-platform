using BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;
using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

public static class PreviewArticleBlockParser
{
    public static IReadOnlyCollection<PreviewArticleBlock> Parse(string? rawJson)
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

        var blocks = new List<PreviewArticleBlock>();

        foreach (var layoutItem in layoutItems.EnumerateArray())
        {
            var contentUdi = layoutItem.GetProperty("contentUdi").GetString();

            if (string.IsNullOrWhiteSpace(contentUdi) ||
                !contentDataByUdi.TryGetValue(contentUdi, out var block))
            {
                continue;
            }

            blocks.Add(PreviewArticleBlockDefinitionRegistry.Parse(block));
        }

        return blocks;
    }
}

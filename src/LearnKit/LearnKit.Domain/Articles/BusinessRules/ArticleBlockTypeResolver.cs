using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Domain.Articles.BusinessRules;

public static class ArticleBlockTypeResolver
{
    private static readonly IReadOnlyDictionary<string, ArticleBlockType> Aliases =
        new Dictionary<string, ArticleBlockType>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(ArticleBlockType.Markdown)] = ArticleBlockType.Markdown,
            ["heading"] = ArticleBlockType.Markdown,
            ["text"] = ArticleBlockType.Markdown,
            [nameof(ArticleBlockType.Code)] = ArticleBlockType.Code,
            ["codeSnippet"] = ArticleBlockType.Code,
            [nameof(ArticleBlockType.Diagram)] = ArticleBlockType.Diagram,
            ["plantUml"] = ArticleBlockType.Diagram,
            ["plantUmlDiagram"] = ArticleBlockType.Diagram,
            ["mermaid"] = ArticleBlockType.Diagram,
            ["mermaidDiagram"] = ArticleBlockType.Diagram,
            [nameof(ArticleBlockType.Table)] = ArticleBlockType.Table,
            [nameof(ArticleBlockType.Callout)] = ArticleBlockType.Callout,
            [nameof(ArticleBlockType.Summary)] = ArticleBlockType.Summary
        };

    public static bool TryResolve(string? value, out ArticleBlockType blockType)
    {
        blockType = default;

        return !string.IsNullOrWhiteSpace(value) &&
               Aliases.TryGetValue(value.Trim(), out blockType);
    }

    public static ArticleBlockType Resolve(string value)
    {
        if (TryResolve(value, out var blockType))
        {
            return blockType;
        }

        throw new ArgumentException($"Unsupported LearnKit block type '{value}'.", nameof(value));
    }

    public static string GetCanonicalName(string value) => Resolve(value).ToString();
}

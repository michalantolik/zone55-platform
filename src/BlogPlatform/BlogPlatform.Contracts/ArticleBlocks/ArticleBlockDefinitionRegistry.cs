using System.Text.Json;

namespace BlogPlatform.Contracts.Posts.ArticleBlocks;

public static class ArticleBlockDefinitionRegistry
{
    private static readonly IReadOnlyList<IArticleBlockDefinition> Definitions =
    [
        new CodeArticleBlockDefinition(),
        new PlantUmlArticleBlockDefinition(),
        new MermaidArticleBlockDefinition(),
        new TableArticleBlockDefinition(),
        new SummaryArticleBlockDefinition(),
        new CalloutArticleBlockDefinition(),
        new HeadingArticleBlockDefinition(),
        new TextArticleBlockDefinition()
    ];

    public static IReadOnlyList<IArticleBlockDefinition> All => Definitions;

    public static ArticleBlockDto Parse(JsonElement block)
    {
        var definition = Definitions.FirstOrDefault(definition => definition.CanParse(block));

        if (definition is null)
        {
            return new ArticleBlockDto(ArticleBlockType.Text)
            {
                Text = ArticleBlockJsonReader.GetString(block, ArticleBlockPropertyNames.Text)
            };
        }

        return definition.Parse(block);
    }

    public static IArticleBlockDefinition GetRequired(ArticleBlockType type)
    {
        return Definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException(
                $"No article block definition registered for '{type}'.");
    }
}

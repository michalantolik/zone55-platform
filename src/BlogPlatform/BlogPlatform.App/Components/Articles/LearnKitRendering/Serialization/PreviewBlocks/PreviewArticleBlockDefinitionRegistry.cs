using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

public static class PreviewArticleBlockDefinitionRegistry
{
    private static readonly IReadOnlyList<IPreviewArticleBlockDefinition> Definitions =
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

    public static IReadOnlyList<IPreviewArticleBlockDefinition> All => Definitions;

    public static PreviewArticleBlock Parse(JsonElement block)
    {
        var definition = Definitions.FirstOrDefault(definition => definition.CanParse(block));

        if (definition is null)
        {
            return new PreviewArticleBlock(PreviewArticleBlockType.Text)
            {
                Text = PreviewArticleBlockJsonReader.GetString(block, PreviewArticleBlockPropertyNames.Text)
            };
        }

        return definition.Parse(block);
    }

    public static IPreviewArticleBlockDefinition GetRequired(PreviewArticleBlockType type)
    {
        return Definitions.FirstOrDefault(definition => definition.Type == type)
            ?? throw new InvalidOperationException(
                $"No article block definition registered for '{type}'.");
    }
}

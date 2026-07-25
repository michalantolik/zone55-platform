using System.Text.Json;

namespace Zone55.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

public interface IPreviewArticleBlockDefinition
{
    PreviewArticleBlockType Type { get; }

    string DisplayName { get; }

    string EditorType { get; }

    IReadOnlyCollection<string> UmbracoAliases { get; }

    bool CanParse(JsonElement block);

    PreviewArticleBlock Parse(JsonElement block);
}

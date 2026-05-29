using System.Text.Json;

namespace BlogPlatform.Contracts.Posts.ArticleBlocks;

public interface IArticleBlockDefinition
{
    ArticleBlockType Type { get; }

    string DisplayName { get; }

    string EditorType { get; }

    IReadOnlyCollection<string> UmbracoAliases { get; }

    bool CanParse(JsonElement block);

    ArticleBlockDto Parse(JsonElement block);
}

using System.Text.Json;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one article block stored in the content seed.
/// </summary>
public sealed class ArticleBlockSeed
{
    /// <summary>
    /// Block type, for example Markdown, Code, Diagram or Table.
    /// </summary>
    public ArticleBlockType Type { get; init; }

    /// <summary>
    /// Determines the display order within the article.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Type-specific block content.
    /// </summary>
    public JsonElement Content { get; init; }

    /// <summary>
    /// Language-specific block data. Empty for legacy schema v1 seeds.
    /// </summary>
    public Dictionary<string, ArticleBlockTranslationSeed> Translations { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}

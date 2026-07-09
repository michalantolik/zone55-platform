using System.Text.Json;
using LearnKit.Domain.Articles;

namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Represents one article block stored in the content seed.
/// </summary>
internal sealed class ArticleBlockSeed
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
}

namespace LearnKit.Application.Articles.Public.Models;

/// <summary>
/// Represents article block data prepared for reading.
/// </summary>
/// <param name="Id">
/// Unique block identifier.
/// </param>
/// <param name="Type">
/// Block type used by the renderer.
/// </param>
/// <param name="SortOrder">
/// Block position inside the article.
/// </param>
/// <param name="ContentJson">
/// Block content stored as JSON.
/// </param>
public sealed record ArticleBlockDetails(
    Guid Id,
    string Type,
    int SortOrder,
    string ContentJson);

namespace LearnKit.Application.Articles.Public.Models;

/// <summary>
/// Represents article data prepared for reading.
/// </summary>
/// <param name="Id">
/// Unique article identifier.
/// </param>
/// <param name="Slug">
/// URL-friendly article identifier.
/// </param>
/// <param name="Title">
/// Article title shown to the user.
/// </param>
/// <param name="Summary">
/// Short article description.
/// </param>
/// <param name="Status">
/// Current article publishing state.
/// </param>
/// <param name="Blocks">
/// Ordered article blocks.
/// </param>
public sealed record ArticleDetails(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Status,
    IReadOnlyCollection<ArticleBlockDetails> Blocks);

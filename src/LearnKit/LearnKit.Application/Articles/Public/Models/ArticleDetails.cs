namespace LearnKit.Application.Articles.Public.Models;

/// <summary>
/// Represents article data prepared for reading.
/// </summary>
/// <param name="Id">
/// Unique article identifier.
/// </param>
/// <param name="Slug">
/// URL-friendly identifier shared by every language.
/// </param>
/// <param name="Title">
/// Article title in the resolved language.
/// </param>
/// <param name="Summary">
/// Article summary in the resolved language.
/// </param>
/// <param name="Status">
/// Publishing state of the resolved language version.
/// </param>
/// <param name="Blocks">
/// Ordered blocks in the resolved language.
/// </param>
/// <param name="LanguageCode">
/// Language actually returned by the API.
/// </param>
/// <param name="IsFallback">
/// Indicates that the requested language was unavailable
/// and the default language was returned.
/// </param>
public sealed record ArticleDetails(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Status,
    IReadOnlyCollection<ArticleBlockDetails> Blocks,
    string LanguageCode = "en",
    bool IsFallback = false);

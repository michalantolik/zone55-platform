namespace Backend55.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Language-specific article details accepted
/// by the translation update endpoint.
/// </summary>
/// <param name="Title">
/// Translated article title.
/// </param>
/// <param name="Summary">
/// Translated article summary.
/// </param>
public sealed record UpdateArticleTranslationRequest(
    string Title,
    string? Summary);

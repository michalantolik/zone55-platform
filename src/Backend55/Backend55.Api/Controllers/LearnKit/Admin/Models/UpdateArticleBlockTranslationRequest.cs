namespace Backend55.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Language-specific block content accepted
/// by the translation update endpoint.
/// </summary>
/// <param name="ContentJson">
/// Translated block content.
/// </param>
public sealed record UpdateArticleBlockTranslationRequest(
    string ContentJson);

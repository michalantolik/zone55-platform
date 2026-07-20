namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Article block details accepted by the update endpoint.
/// </summary>
public sealed record UpdateArticleBlockRequest(
    string Type,
    string ContentJson);

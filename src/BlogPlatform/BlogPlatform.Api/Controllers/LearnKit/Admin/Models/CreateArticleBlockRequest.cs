namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Article block details accepted by the create endpoint.
/// </summary>
public sealed record CreateArticleBlockRequest(
    string Type,
    int SortOrder,
    string ContentJson);

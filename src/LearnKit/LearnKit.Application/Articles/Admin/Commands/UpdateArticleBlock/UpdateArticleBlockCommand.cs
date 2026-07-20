namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;

/// <summary>
/// Represents a request to update an article block.
/// </summary>
public sealed record UpdateArticleBlockCommand(
    Guid ArticleId,
    Guid BlockId,
    string Type,
    string ContentJson);

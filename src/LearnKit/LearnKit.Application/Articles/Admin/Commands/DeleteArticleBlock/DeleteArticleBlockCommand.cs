namespace LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;

/// <summary>
/// Represents a request to delete an article block.
/// </summary>
public sealed record DeleteArticleBlockCommand(
    Guid ArticleId,
    Guid BlockId);

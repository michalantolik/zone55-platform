namespace LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;

/// <summary>
/// Represents a request to add a block to an article.
/// </summary>
public sealed record CreateArticleBlockCommand(
    Guid ArticleId,
    string Type,
    int SortOrder,
    string ContentJson);

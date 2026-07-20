namespace LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;

/// <summary>
/// Represents a request to reorder all blocks in an article.
/// </summary>
public sealed record ReorderArticleBlocksCommand(
    Guid ArticleId,
    IReadOnlyCollection<Guid> OrderedBlockIds);

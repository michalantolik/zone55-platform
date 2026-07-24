using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;

/// <summary>
/// Handles requests to reorder article blocks.
/// </summary>
public sealed class ReorderArticleBlocksHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public ReorderArticleBlocksHandler(IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    public async Task<bool> HandleAsync(
        ReorderArticleBlocksCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return false;
        }

        try
        {
            article.ReorderBlocks(command.OrderedBlockIds);
        }
        catch (ArgumentException)
        {
            return false;
        }
        await _articleManagementStore.SaveChangesAsync(cancellationToken);
        return true;
    }
}

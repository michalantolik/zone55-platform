using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;

/// <summary>
/// Handles requests to delete article blocks.
/// </summary>
public sealed class DeleteArticleBlockHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public DeleteArticleBlockHandler(IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    public async Task<bool> HandleAsync(
        DeleteArticleBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null || !article.RemoveBlock(command.BlockId))
        {
            return false;
        }

        await _articleManagementStore.SaveChangesAsync(cancellationToken);
        return true;
    }
}

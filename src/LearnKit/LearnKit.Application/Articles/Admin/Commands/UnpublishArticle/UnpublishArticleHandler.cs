using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;

/// <summary>
/// Handles requests to unpublish articles.
/// </summary>
public sealed class UnpublishArticleHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public UnpublishArticleHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Moves an article back to draft when it exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the article was found and moved to draft;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleAsync(
        UnpublishArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return false;
        }

        article.MoveToDraft();

        await _articleManagementStore.SaveChangesAsync(cancellationToken);

        return true;
    }
}

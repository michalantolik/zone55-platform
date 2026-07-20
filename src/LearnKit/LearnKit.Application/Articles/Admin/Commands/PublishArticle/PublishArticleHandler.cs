using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.PublishArticle;

/// <summary>
/// Handles requests to publish articles.
/// </summary>
public sealed class PublishArticleHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public PublishArticleHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Publishes an article when it exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the article was found and published;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleAsync(
        PublishArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return false;
        }

        article.Publish();

        await _articleManagementStore.SaveChangesAsync(cancellationToken);

        return true;
    }
}

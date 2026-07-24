using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin;

namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticle;

/// <summary>
/// Handles requests to update basic article details.
/// </summary>
public sealed class UpdateArticleHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public UpdateArticleHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Updates an article when it exists.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the article was found and updated;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleAsync(
        UpdateArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return false;
        }

        if (await _articleManagementStore.SlugExistsAsync(
                command.Slug,
                command.ArticleId,
                cancellationToken))
        {
            throw new ArticleSlugConflictException(command.Slug.Trim());
        }

        article.MoveToStep(command.LearningStepId);
        article.ChangeSlug(command.Slug);
        article.Rename(command.Title);
        article.UpdateSummary(command.Summary);
        article.ChangeSortOrder(command.SortOrder);

        await _articleManagementStore.SaveChangesAsync(cancellationToken);

        return true;
    }
}

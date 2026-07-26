using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleTranslation;

/// <summary>
/// Handles creation and updates of article language versions.
/// </summary>
public sealed class UpdateArticleTranslationHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public UpdateArticleTranslationHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Creates or updates the selected article translation.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the article exists;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleAsync(
        UpdateArticleTranslationCommand command,
        CancellationToken cancellationToken = default)
    {
        var article =
            await _articleManagementStore.GetTrackedByIdAsync(
                command.ArticleId,
                cancellationToken);

        if (article is null)
        {
            return false;
        }

        article.SetTranslation(
            command.LanguageCode,
            command.Title,
            command.Summary);

        await _articleManagementStore.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}

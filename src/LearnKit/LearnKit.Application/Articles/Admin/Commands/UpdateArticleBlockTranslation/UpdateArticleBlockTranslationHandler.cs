using LearnKit.Application.Articles.Admin.Contracts;

namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlockTranslation;

/// <summary>
/// Handles creation and updates of article block translations.
/// </summary>
public sealed class UpdateArticleBlockTranslationHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public UpdateArticleBlockTranslationHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Creates or updates the selected block translation.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when both article and block exist;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public async Task<bool> HandleAsync(
        UpdateArticleBlockTranslationCommand command,
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

        var updated = article.SetBlockTranslation(
            command.BlockId,
            command.LanguageCode,
            command.ContentJson);

        if (!updated)
        {
            return false;
        }

        await _articleManagementStore.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}

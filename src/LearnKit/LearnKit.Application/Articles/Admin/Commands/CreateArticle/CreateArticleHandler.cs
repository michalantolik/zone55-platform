using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Articles.Admin.Commands.CreateArticle;

/// <summary>
/// Handles requests to create draft articles.
/// </summary>
public sealed class CreateArticleHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public CreateArticleHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Creates and persists a new draft article.
    /// </summary>
    /// <returns>
    /// Identifier of the created article.
    /// </returns>
    public async Task<Guid> HandleAsync(
        CreateArticleCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = new Article(
            command.LearningStepId,
            command.Slug,
            command.Title,
            command.SortOrder,
            command.Summary);

        await _articleManagementStore.AddAsync(
            article,
            cancellationToken);

        await _articleManagementStore.SaveChangesAsync(cancellationToken);

        return article.Id;
    }
}

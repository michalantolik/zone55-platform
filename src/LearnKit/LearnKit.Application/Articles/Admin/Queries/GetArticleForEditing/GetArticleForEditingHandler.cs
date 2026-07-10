using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;

namespace LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;

/// <summary>
/// Handles requests to retrieve an article for editing.
/// </summary>
public sealed class GetArticleForEditingHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public GetArticleForEditingHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Retrieves an article for editing.
    /// </summary>
    public Task<ArticleManagementDetails?> HandleAsync(
        GetArticleForEditingQuery query,
        CancellationToken cancellationToken = default)
    {
        return _articleManagementStore.GetByIdAsync(
            query.ArticleId,
            cancellationToken);
    }
}

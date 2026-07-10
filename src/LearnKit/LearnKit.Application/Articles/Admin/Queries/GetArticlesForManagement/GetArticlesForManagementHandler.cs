using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;

namespace LearnKit.Application.Articles.Admin.Queries.GetArticlesForManagement;

/// <summary>
/// Handles requests to retrieve articles for management.
/// </summary>
public sealed class GetArticlesForManagementHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    /// <summary>
    /// Creates a new handler.
    /// </summary>
    public GetArticlesForManagementHandler(
        IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    /// <summary>
    /// Retrieves articles for management.
    /// </summary>
    public Task<IReadOnlyCollection<ArticleManagementListItem>> HandleAsync(
        GetArticlesForManagementQuery query,
        CancellationToken cancellationToken = default)
    {
        return _articleManagementStore.GetAllAsync(
            cancellationToken);
    }
}

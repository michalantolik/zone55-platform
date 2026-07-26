using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Articles.Admin.Contracts;

/// <summary>
/// Provides article data required by article management use cases.
/// </summary>
public interface IArticleManagementStore
{
    /// <summary>
    /// Returns all articles available for management.
    /// </summary>
    Task<IReadOnlyCollection<ArticleManagementListItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns legacy article data for management.
    /// </summary>
    Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an article prepared for editing
    /// in the selected language.
    /// </summary>
    Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        string languageCode,
        CancellationToken cancellationToken = default) =>
        GetByIdAsync(articleId, cancellationToken);

    /// <summary>
    /// Returns a tracked article for a management command.
    /// </summary>
    Task<Article?> GetTrackedByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates whether a slug is already used
    /// by another article.
    /// </summary>
    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingArticleId = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    /// <summary>
    /// Adds a new article to the management store.
    /// </summary>
    Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns tracked articles assigned
    /// to one learning step.
    /// </summary>
    Task<IReadOnlyCollection<Article>> GetTrackedByStepIdAsync(
        Guid learningStepId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an article for removal.
    /// </summary>
    void Remove(Article article);

    /// <summary>
    /// Persists pending article management changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

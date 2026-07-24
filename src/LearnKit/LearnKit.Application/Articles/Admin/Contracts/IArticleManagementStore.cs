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
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Articles ordered for display in the management panel.
    /// </returns>
    Task<IReadOnlyCollection<ArticleManagementListItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an article for management and editing.
    /// </summary>
    /// <param name="articleId">
    /// Unique article identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Article details, or <see langword="null"/> when the article does not exist.
    /// </returns>
    Task<ArticleManagementDetails?> GetByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a tracked article for a management command.
    /// </summary>
    /// <param name="articleId">
    /// Unique article identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The tracked article, or <see langword="null"/> when it does not exist.
    /// </returns>
    Task<Article?> GetTrackedByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Indicates whether a slug is already used by another article.
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
    /// Returns tracked articles assigned to one learning step.
    /// </summary>
    Task<IReadOnlyCollection<Article>> GetTrackedByStepIdAsync(
        Guid learningStepId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an article for removal from the management store.
    /// </summary>
    void Remove(Article article);

    /// <summary>
    /// Persists pending article management changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

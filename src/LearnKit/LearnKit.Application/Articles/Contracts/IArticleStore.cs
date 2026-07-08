using LearnKit.Domain.Articles;

namespace LearnKit.Application.Articles.Contracts;

/// <summary>
/// Provides access to article storage.
///
/// Implementations are responsible for loading
/// and saving articles.
/// </summary>
public interface IArticleStore
{
    /// <summary>
    /// Gets an article by its identifier.
    /// </summary>
    Task<Article?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an article by its slug.
    /// </summary>
    Task<Article?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new article.
    /// </summary>
    Task AddAsync(
        Article article,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing article.
    /// </summary>
    Task RemoveAsync(
        Article article,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a slug already exists.
    /// </summary>
    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves pending changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

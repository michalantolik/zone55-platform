using LearnKit.Domain.Roadmaps;

namespace LearnKit.Application.Roadmaps.Contracts;

/// <summary>
/// Provides access to learning path storage.
///
/// Implementations are responsible for loading
/// and saving learning paths.
/// </summary>
public interface ILearningPathStore
{
    /// <summary>
    /// Gets a learning path by its identifier.
    /// </summary>
    Task<LearningPath?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a learning path by its key.
    /// </summary>
    Task<LearningPath?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new learning path.
    /// </summary>
    Task AddAsync(
        LearningPath learningPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing learning path.
    /// </summary>
    Task RemoveAsync(
        LearningPath learningPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a key already exists.
    /// </summary>
    Task<bool> KeyExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves pending changes.
    /// </summary>
    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

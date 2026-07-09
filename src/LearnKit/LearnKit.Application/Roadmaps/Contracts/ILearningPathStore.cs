using LearnKit.Application.Roadmaps.Models;

namespace LearnKit.Application.Roadmaps.Contracts;

/// <summary>
/// Provides read access to learning paths.
/// </summary>
public interface ILearningPathStore
{
    /// <summary>
    /// Gets a learning path by its stable key.
    /// </summary>
    Task<LearningPathDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

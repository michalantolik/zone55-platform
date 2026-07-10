using LearnKit.Application.Roadmaps.Public.Contracts;
using LearnKit.Application.Roadmaps.Public.Models;

namespace LearnKit.Application.Roadmaps.Public.Queries.GetLearningPath;

/// <summary>
/// Handles loading a learning path by key.
/// </summary>
public sealed class GetLearningPathHandler
{
    private readonly ILearningPathStore _learningPathStore;

    public GetLearningPathHandler(ILearningPathStore learningPathStore)
    {
        _learningPathStore = learningPathStore;
    }

    public async Task<LearningPathDetails?> HandleAsync(
        GetLearningPathQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _learningPathStore.GetByKeyAsync(
            query.Key,
            cancellationToken);
    }
}

using LearnKit.Application.Roadmaps.Public.Contracts;
using LearnKit.Application.Roadmaps.Public.Models;

namespace LearnKit.Application.Roadmaps.Public.Queries.GetFirstLearningPath;

public sealed class GetFirstLearningPathHandler(ILearningPathStore store)
{
    public Task<LearningPathDetails?> HandleAsync(GetFirstLearningPathQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return store.GetFirstAsync(cancellationToken);
    }
}

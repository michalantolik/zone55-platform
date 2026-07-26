using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;

namespace LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathsForManagement;

public sealed class GetLearningPathsForManagementHandler(
    ILearningPathManagementStore store)
{
    public Task<IReadOnlyCollection<LearningPathManagementListItem>> HandleAsync(
        GetLearningPathsForManagementQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return store.GetAllAsync(cancellationToken);
    }
}

using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;

namespace LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;

public sealed class GetLearningPathForManagementHandler
{
    private readonly ILearningPathManagementStore _store;

    public GetLearningPathForManagementHandler(ILearningPathManagementStore store)
    {
        _store = store;
    }

    public Task<LearningPathManagementDetails?> HandleAsync(
        GetLearningPathForManagementQuery query,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByKeyAsync(query.Key, cancellationToken);
    }
}

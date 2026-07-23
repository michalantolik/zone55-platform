using LearnKit.Application.Roadmaps.Admin.Contracts;

namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;

public sealed class UpdateLearningPathHandler
{
    private readonly ILearningPathManagementStore _store;

    public UpdateLearningPathHandler(ILearningPathManagementStore store)
    {
        _store = store;
    }

    public async Task<bool> HandleAsync(
        UpdateLearningPathCommand command,
        CancellationToken cancellationToken = default)
    {
        var learningPath = await _store.GetTrackedPathByIdAsync(
            command.LearningPathId,
            cancellationToken);

        if (learningPath is null)
        {
            return false;
        }

        learningPath.Rename(command.Title);
        learningPath.UpdateSummary(command.Summary);

        await _store.SaveChangesAsync(cancellationToken);

        return true;
    }
}

using LearnKit.Application.Roadmaps.Admin.Contracts;

namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;

public sealed class UpdateLearningZoneHandler
{
    private readonly ILearningPathManagementStore _store;

    public UpdateLearningZoneHandler(ILearningPathManagementStore store)
    {
        _store = store;
    }

    public async Task<bool> HandleAsync(
        UpdateLearningZoneCommand command,
        CancellationToken cancellationToken = default)
    {
        var learningZone = await _store.GetTrackedZoneByIdAsync(
            command.LearningZoneId,
            cancellationToken);

        if (learningZone is null)
        {
            return false;
        }

        learningZone.Rename(command.Title);
        learningZone.UpdateSummary(command.Summary);

        await _store.SaveChangesAsync(cancellationToken);

        return true;
    }
}

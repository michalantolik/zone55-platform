using LearnKit.Application.Roadmaps.Admin.Contracts;

namespace LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;

public sealed class UpdateLearningStepHandler
{
    private readonly ILearningPathManagementStore _store;

    public UpdateLearningStepHandler(ILearningPathManagementStore store)
    {
        _store = store;
    }

    public async Task<bool> HandleAsync(
        UpdateLearningStepCommand command,
        CancellationToken cancellationToken = default)
    {
        var learningStep = await _store.GetTrackedStepByIdAsync(
            command.LearningStepId,
            cancellationToken);

        if (learningStep is null)
        {
            return false;
        }

        learningStep.Rename(command.Title);
        learningStep.UpdateSummary(command.Summary);

        await _store.SaveChangesAsync(cancellationToken);

        return true;
    }
}

using LearnKit.Application.Roadmaps.Admin;
using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Domain.Roadmaps;

namespace LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningPath;

public sealed class CreateLearningPathHandler(
    ILearningPathManagementStore store)
{
    public async Task<Guid> HandleAsync(
        CreateLearningPathCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var key = command.Key?.Trim() ?? string.Empty;

        if (await store.PathKeyExistsAsync(key, cancellationToken))
        {
            throw new LearningStructureKeyConflictException(
                "learning path",
                key);
        }

        var learningPath = new LearningPath(
            key,
            command.Title,
            command.Summary);

        store.Add(learningPath);
        await store.SaveChangesAsync(cancellationToken);

        return learningPath.Id;
    }
}

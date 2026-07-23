namespace LearnKit.Application.Roadmaps.Admin.Contracts;

using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Domain.Roadmaps;

public interface ILearningPathManagementStore
{
    Task<LearningPathManagementDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<LearningPath?> GetTrackedPathByIdAsync(
        Guid learningPathId,
        CancellationToken cancellationToken = default);

    Task<LearningZone?> GetTrackedZoneByIdAsync(
        Guid learningZoneId,
        CancellationToken cancellationToken = default);

    Task<LearningStep?> GetTrackedStepByIdAsync(
        Guid learningStepId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

namespace LearnKit.Application.Roadmaps.Admin.Contracts;

using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Domain.Roadmaps;

public interface ILearningPathManagementStore
{
    Task<IReadOnlyCollection<LearningPathManagementListItem>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<LearningPathManagementDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<int> GetNextPathSortOrderAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(1);

    Task<bool> PathKeyExistsAsync(
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

    Task<bool> ZoneKeyExistsAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    Task<bool> StepKeyExistsAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    void Add(LearningPath learningPath);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

namespace LearnKit.Application.Roadmaps.Admin.Contracts;

using LearnKit.Application.Roadmaps.Admin.Models;

public interface ILearningPathManagementStore
{
    Task<LearningPathManagementDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

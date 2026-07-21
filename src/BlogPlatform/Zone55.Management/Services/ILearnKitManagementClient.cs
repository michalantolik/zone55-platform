using Zone55.Management.Models;

namespace Zone55.Management.Services;

public interface ILearnKitManagementClient
{
    Task<IReadOnlyCollection<ArticleManagementListItem>> GetArticlesAsync(
        CancellationToken cancellationToken = default);

    Task<LearningPathManagementDetails?> GetLearningPathAsync(
        string key,
        CancellationToken cancellationToken = default);
}

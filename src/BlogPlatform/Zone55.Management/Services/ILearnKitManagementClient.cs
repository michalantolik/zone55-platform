using Zone55.Management.Models;

namespace Zone55.Management.Services;

public interface ILearnKitManagementClient
{
    Task<IReadOnlyCollection<ArticleManagementListItem>> GetArticlesAsync(
        CancellationToken cancellationToken = default);

    Task<ArticleManagementDetails?> GetArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    Task<LearningPathManagementDetails?> GetLearningPathAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task UpdateArticleAsync(
        Guid articleId,
        UpdateArticleManagementRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateArticleBlockAsync(
        Guid articleId,
        Guid blockId,
        UpdateArticleBlockManagementRequest request,
        CancellationToken cancellationToken = default);

    Task PublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    Task UnpublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);
}

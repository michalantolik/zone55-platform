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

    Task UpdateLearningPathAsync(
        Guid learningPathId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateLearningZoneAsync(
        Guid learningZoneId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateLearningStepAsync(
        Guid learningStepId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateArticleAsync(
        CreateArticleManagementRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateArticleAsync(
        Guid articleId,
        UpdateArticleManagementRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateArticleBlockAsync(
        Guid articleId,
        CreateArticleBlockManagementRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateArticleBlockAsync(
        Guid articleId,
        Guid blockId,
        UpdateArticleBlockManagementRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteArticleBlockAsync(
        Guid articleId,
        Guid blockId,
        CancellationToken cancellationToken = default);

    Task ReorderArticleBlocksAsync(
        Guid articleId,
        ReorderArticleBlocksManagementRequest request,
        CancellationToken cancellationToken = default);

    Task PublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);

    Task UnpublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default);
}

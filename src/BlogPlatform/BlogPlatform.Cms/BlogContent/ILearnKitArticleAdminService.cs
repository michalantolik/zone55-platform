namespace BlogPlatform.Cms.BlogContent;
public interface ILearnKitArticleAdminService
{
    Task<IReadOnlyCollection<CmsArticleListItemDto>> GetArticlesAsync(CancellationToken cancellationToken = default);
    Task<CmsArticleEditorDto?> GetArticleAsync(Guid key, CancellationToken cancellationToken = default);
    Task<int> GetNextArticleOrderAsync(string? zone, string? step, Guid? excludeArticleKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CmsReorderArticleListItemDto>> GetArticlesForReorderAsync(string? zone, string? step, CancellationToken cancellationToken = default);
    Task<CmsReorderArticlesResponse> ReorderArticlesAsync(CmsReorderArticlesRequest request, CancellationToken cancellationToken = default);
    Task<CmsSaveArticleResponse> CreateArticleAsync(CmsSaveArticleRequest request, CancellationToken cancellationToken = default);
    Task<CmsSaveArticleResponse> UpdateArticleAsync(Guid key, CmsSaveArticleRequest request, CancellationToken cancellationToken = default);
    Task<CmsDeleteResponse> DeleteArticleAsync(Guid key, CancellationToken cancellationToken = default);
}

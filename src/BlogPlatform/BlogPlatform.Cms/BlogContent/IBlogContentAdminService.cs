namespace BlogPlatform.Cms.BlogContent;

public interface IBlogContentAdminService
{
    Task<IReadOnlyCollection<CmsDotnetZoneListItemDto>> GetDotnetRoadmapAsync(
        CancellationToken cancellationToken);

    IReadOnlyCollection<CmsDocumentTypeListItemDto> GetDocumentTypes();

    IReadOnlyCollection<CmsArticleListItemDto> GetArticles();

    CmsArticleEditorDto? GetArticle(Guid key);

    Task<CmsSaveArticleResponse> CreateArticleAsync(
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken);

    Task<CmsSaveArticleResponse> UpdateArticleAsync(
        Guid key,
        CmsSaveArticleRequest request,
        CancellationToken cancellationToken);

    CmsDeleteResponse DeleteArticle(Guid key);

    CmsDeleteResponse DeleteDocumentType(Guid key);

    Task<CmsSaveRoadmapResponse> CreateZoneAsync(
        CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken);

    Task<CmsSaveRoadmapResponse> UpdateZoneAsync(
        string zoneKey,
        CmsSaveRoadmapZoneRequest request,
        CancellationToken cancellationToken);

    Task<CmsDeleteResponse> DeleteZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken);

    Task<CmsSaveRoadmapResponse> CreateStepAsync(
        string zoneKey,
        CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken);

    Task<CmsSaveRoadmapResponse> UpdateStepAsync(
        string zoneKey,
        string stepKey,
        CmsSaveRoadmapStepRequest request,
        CancellationToken cancellationToken);

    Task<CmsDeleteResponse> DeleteStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken);
}

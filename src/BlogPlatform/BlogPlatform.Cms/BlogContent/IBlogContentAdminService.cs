using BlogPlatform.Cms.Seeding;

namespace BlogPlatform.Cms.BlogContent;

public interface IBlogContentAdminService
{
    Task<IReadOnlyCollection<CmsDotnetZoneListItemDto>> GetDotnetRoadmapAsync(
        CancellationToken cancellationToken);

    Task<CmsDatabaseSummaryDto> GetDatabaseSummaryAsync(
        CancellationToken cancellationToken);

    Task<BlogSeedContent> BuildSeedContentAsync(
        CancellationToken cancellationToken);

    IReadOnlyCollection<CmsDocumentTypeListItemDto> GetDocumentTypes();

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

    Task<CmsSeedImportResponse> ImportSeedContentAsync(
        CancellationToken cancellationToken);
}

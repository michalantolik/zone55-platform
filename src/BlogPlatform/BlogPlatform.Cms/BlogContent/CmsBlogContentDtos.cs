namespace BlogPlatform.Cms.BlogContent;

public sealed record CmsDotnetZoneListItemDto(
    string Key,
    string Name,
    int Order,
    int ArticleCount,
    IReadOnlyCollection<CmsDotnetZoneStepListItemDto> Steps);

public sealed record CmsDotnetZoneStepListItemDto(
    string Key,
    string Name,
    int Order,
    int ArticleCount);

public sealed record CmsDocumentTypeListItemDto(
    Guid Key,
    string Name,
    string Alias,
    string Icon,
    bool IsElement,
    int ContentCount);

public sealed record CmsArticleListItemDto(
    Guid Key,
    string Slug,
    string Title,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    int Order,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset? PublishedDate,
    string BodyHtml,
    DateTime UpdatedUtc);

public sealed record CmsArticleEditorDto(
    Guid Key,
    string Title,
    string Slug,
    string Summary,
    string Level,
    string Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    int Order,
    string Tags,
    string BodyBlocks,
    bool IsExistingArticle);

public sealed record CmsSaveArticleRequest(
    string Title,
    string Slug,
    string Summary,
    string Tags,
    string BodyBlocks,
    string? Level,
    string? Focus,
    string? DotnetZone,
    string? DotnetZoneStep,
    int? Order);

public sealed record CmsSaveArticleResponse(
    bool Success,
    Guid Key,
    string Message);

public sealed record CmsSaveRoadmapZoneRequest(
    string Name,
    string? Key);

public sealed record CmsSaveRoadmapStepRequest(
    string Name,
    string? Key);

public sealed record CmsSaveRoadmapResponse(
    bool Success,
    string Message);

public sealed record CmsDeleteResponse(
    bool Success,
    string Message);


public sealed record CmsReorderArticleListItemDto(
    Guid Key,
    string Title,
    string Slug,
    string Summary,
    string Level,
    string Focus,
    string DotnetZone,
    string DotnetZoneStep,
    int Order,
    DateTime UpdatedUtc);

public sealed record CmsReorderArticlesRequest(
    string DotnetZone,
    string DotnetZoneStep,
    IReadOnlyCollection<Guid> ArticleKeys);

public sealed record CmsReorderArticlesResponse(
    bool Success,
    string Message,
    IReadOnlyCollection<CmsReorderArticleListItemDto> Articles);

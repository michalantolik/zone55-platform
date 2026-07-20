namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;

/// <summary>
/// Complete article block order accepted by the reorder endpoint.
/// </summary>
public sealed record ReorderArticleBlocksRequest(
    IReadOnlyCollection<Guid> OrderedBlockIds);

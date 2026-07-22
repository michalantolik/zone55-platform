namespace Zone55.Management.Models;

public sealed record ReorderArticleBlocksManagementRequest(
    IReadOnlyCollection<Guid> OrderedBlockIds);

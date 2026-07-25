namespace Zone55.Management.Models;

public sealed record CreateArticleBlockManagementRequest(
    string Type,
    int SortOrder,
    string ContentJson);

namespace Zone55.Management.Models;

public sealed record UpdateArticleBlockManagementRequest(
    string Type,
    string ContentJson);

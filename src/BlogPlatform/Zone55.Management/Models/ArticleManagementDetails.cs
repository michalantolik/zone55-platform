namespace Zone55.Management.Models;

public sealed record ArticleManagementDetails(
    Guid Id,
    Guid LearningStepId,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    IReadOnlyCollection<ArticleBlockDetails> Blocks);

public sealed record ArticleBlockDetails(
    Guid Id,
    string Type,
    int SortOrder,
    string ContentJson);

namespace LearnKit.Application.Content.Admin.Models;

public sealed record LearnKitContentValidationReport(
    bool IsValid,
    DateTimeOffset ValidatedAtUtc,
    LearnKitContentCounts Counts,
    IReadOnlyCollection<LearnKitContentValidationIssue> Issues);

public sealed record LearnKitContentCounts(
    int Paths,
    int Zones,
    int Steps,
    int Articles,
    int Blocks);

public sealed record LearnKitContentValidationIssue(
    string Severity,
    string Code,
    string Location,
    string Message);

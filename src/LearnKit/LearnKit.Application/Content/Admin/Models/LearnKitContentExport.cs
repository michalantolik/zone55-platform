using System.Text.Json;

namespace LearnKit.Application.Content.Admin.Models;

public sealed record LearnKitContentExport(
    int SchemaVersion,
    DateTimeOffset ExportedAtUtc,
    IReadOnlyCollection<LearningPathExport> Paths);

public sealed record LearningPathExport(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    IReadOnlyCollection<LearningZoneExport> Zones);

public sealed record LearningZoneExport(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<LearningStepExport> Steps);

public sealed record LearningStepExport(
    Guid Id,
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<ArticleExport> Articles);

public sealed record ArticleExport(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    int SortOrder,
    string Status,
    IReadOnlyCollection<ArticleBlockExport> Blocks);

public sealed record ArticleBlockExport(
    Guid Id,
    string Type,
    int SortOrder,
    JsonElement Content);

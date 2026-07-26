using System.Text.Json;

namespace LearnKit.Application.Content.Admin.Models;

/// <summary>
/// Represents LearnKit content in the same shape as an application seed file.
/// </summary>
public sealed record LearnKitSeedExport(
    int SchemaVersion,
    LearnKitSeedContent Content);

public sealed record LearnKitSeedContent(
    IReadOnlyCollection<LearningPathSeedExport> LearningPaths);

public sealed record LearningPathSeedExport(
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<LearningZoneSeedExport> Zones);

public sealed record LearningZoneSeedExport(
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<LearningStepSeedExport> Steps);

public sealed record LearningStepSeedExport(
    string Key,
    string Title,
    string Summary,
    int SortOrder,
    IReadOnlyCollection<ArticleSeedExport> Articles);

public sealed record ArticleSeedExport(
    string Slug,
    string Title,
    string Summary,
    int Status,
    int SortOrder,
    IReadOnlyCollection<ArticleBlockSeedExport> Blocks);

public sealed record ArticleBlockSeedExport(
    int Type,
    int SortOrder,
    JsonElement Content);

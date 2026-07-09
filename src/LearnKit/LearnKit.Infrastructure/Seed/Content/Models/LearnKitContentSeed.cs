namespace LearnKit.Infrastructure.Seed.Content.Models;

/// <summary>
/// Root model of the LearnKit content seed file.
/// </summary>
internal sealed class LearnKitContentSeed
{
    /// <summary>
    /// Version of the seed file schema.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Content stored in the seed file.
    /// </summary>
    public LearnKitContentSeedBody Content { get; init; } = new();
}

using System.Text.Json;
using LearnKit.Infrastructure.Seed.Content.Models;

namespace LearnKit.Infrastructure.Seed.Content;

/// <summary>
/// Loads LearnKit content from the seed file.
/// </summary>
public sealed class LearnKitContentSeedLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Loads the content seed from the specified JSON file.
    /// </summary>
    /// <param name="path">
    /// Path to the seed file.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task<LearnKitContentSeed> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await using var stream = File.OpenRead(path);

        var seed = await JsonSerializer.DeserializeAsync<LearnKitContentSeed>(
            stream,
            SerializerOptions,
            cancellationToken);

        if (seed is null)
        {
            throw new InvalidOperationException(
                "The LearnKit content seed file is empty or invalid.");
        }

        return seed;
    }
}

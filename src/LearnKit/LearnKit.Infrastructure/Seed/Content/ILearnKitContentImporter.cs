using LearnKit.Infrastructure.Seed.Content.Models;

namespace LearnKit.Infrastructure.Seed.Content;

/// <summary>
/// Imports LearnKit content from a seed model into the database.
/// </summary>
internal interface ILearnKitContentImporter
{
    /// <summary>
    /// Imports the specified LearnKit content.
    /// </summary>
    /// <param name="seed">
    /// LearnKit content loaded from the seed file.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    Task ImportAsync(
        LearnKitContentSeed seed,
        CancellationToken cancellationToken = default);
}

namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Describes the product-specific content used for the initial LearnKit bootstrap.
/// </summary>
public interface ILearnKitContentSeedSource
{
    /// <summary>
    /// Opens the product-specific content seed.
    /// </summary>
    Stream OpenRead();

    /// <summary>
    /// Gets the stable version recorded with the database initialization.
    /// </summary>
    string SourceVersion { get; }
}

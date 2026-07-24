namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Describes how LearnKit content initialization was completed.
/// </summary>
public enum LearnKitInitializationResult
{
    Seeded,
    ExistingContentAdopted,
    AlreadyInitialized
}

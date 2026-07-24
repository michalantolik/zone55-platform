namespace LearnKit.Infrastructure.Seed;

/// <summary>
/// Records that the initial LearnKit content bootstrap has been completed.
/// </summary>
public sealed class LearnKitInitializationRecord
{
    private LearnKitInitializationRecord()
    {
    }

    public LearnKitInitializationRecord(
        string key,
        DateTimeOffset appliedAt,
        string sourceVersion)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Initialization key is required.", nameof(key))
            : key.Trim();
        AppliedAt = appliedAt;
        SourceVersion = string.IsNullOrWhiteSpace(sourceVersion)
            ? throw new ArgumentException("Source version is required.", nameof(sourceVersion))
            : sourceVersion.Trim();
    }

    public string Key { get; private set; } = null!;

    public DateTimeOffset AppliedAt { get; private set; }

    public string SourceVersion { get; private set; } = null!;
}

namespace Backend55.Portal.Services;

/// <summary>
/// Provides users available for local development.
/// </summary>
public static class DevelopmentUsers
{
    /// <summary>
    /// Name of the HTTP header carrying the selected development user.
    /// </summary>
    public const string HeaderName = "X-Backend55-User-Id";

    /// <summary>
    /// Identifier of the default development user.
    /// </summary>
    public static readonly Guid MichalId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Identifier of the secondary development user.
    /// </summary>
    public static readonly Guid MarcinId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>
    /// User selected when no previous selection exists.
    /// </summary>
    public static DevelopmentUser Default { get; } =
        new(MichalId, "Michał");

    /// <summary>
    /// Users available in the local development environment.
    /// </summary>
    public static IReadOnlyList<DevelopmentUser> All { get; } =
    [
        Default,
        new(MarcinId, "Marcin")
    ];

    /// <summary>
    /// Returns whether the specified identifier belongs to a supported
    /// development user.
    /// </summary>
    public static bool Contains(Guid userId) =>
        All.Any(user => user.Id == userId);
}

/// <summary>
/// Represents a user available for local development.
/// </summary>
public sealed record DevelopmentUser(
    Guid Id,
    string DisplayName);

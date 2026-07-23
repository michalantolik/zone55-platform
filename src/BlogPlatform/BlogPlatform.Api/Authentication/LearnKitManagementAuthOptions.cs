namespace BlogPlatform.Api.Authentication;

public sealed class LearnKitManagementAuthOptions
{
    public const string SectionName = "LearnKitManagementAuth";
    public const string PolicyName = "LearnKitManagement";

    public string Username { get; init; } = string.Empty;
    public string PasswordSha256 { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int TokenLifetimeMinutes { get; init; } = 60;
}

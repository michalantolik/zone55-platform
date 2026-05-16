namespace BlogPlatform.Application.Posts;

public sealed record GetPublishedPostsForStepQuery(
    string? DotnetZone,
    string? DotnetZoneStep)
{
    public string? NormalizedDotnetZone =>
        string.IsNullOrWhiteSpace(DotnetZone)
            ? null
            : DotnetZone.Trim();

    public string? NormalizedDotnetZoneStep =>
        string.IsNullOrWhiteSpace(DotnetZoneStep)
            ? null
            : DotnetZoneStep.Trim();
}

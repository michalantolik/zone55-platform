namespace BlogPlatform.Application.Posts;

public sealed record GetPublishedPostsQuery
{
    public static GetPublishedPostsQuery All { get; } = new();
}

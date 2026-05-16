using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Posts;

internal static class BlogPostApplicationMapper
{
    public static PostListItem ToListItem(Post post)
    {
        return new PostListItem(
            post.Slug,
            post.Title,
            post.Summary,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate);
    }

    public static PostDetails ToDetails(Post post)
    {
        return new PostDetails(
            post.Slug,
            post.Title,
            post.Summary,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate,
            post.BodyHtml);
    }
}

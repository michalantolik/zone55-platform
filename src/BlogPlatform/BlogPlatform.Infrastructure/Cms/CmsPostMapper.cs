using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Infrastructure.Cms;

internal static class CmsPostMapper
{
    public static Post ToDomainPost(CmsPostDto post)
    {
        return Post.CreatePublished(
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

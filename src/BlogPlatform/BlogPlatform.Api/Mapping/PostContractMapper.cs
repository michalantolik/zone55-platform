using BlogPlatform.Application.Posts;
using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.Api.Mapping;

internal static class PostContractMapper
{
    public static PostListItemDto ToDto(PostListItem post)
    {
        return new PostListItemDto(
            post.Slug,
            post.Title,
            post.Summary,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Order,
            post.Tags,
            post.PublishedDate);
    }

    public static PostDetailsDto ToDto(PostDetails post)
    {
        return new PostDetailsDto(
            post.Slug,
            post.Title,
            post.Summary,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Order,
            post.Tags,
            post.PublishedDate,
            post.BodyHtml);
    }
}

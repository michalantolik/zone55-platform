using BlogPlatform.App.Models;
using BlogPlatform.Contracts.DotnetRoadmap;

namespace BlogPlatform.App.Services;

public interface IBlogApiClient
{
    Task<BlogHomeContent> GetHomeContentAsync(
        CancellationToken cancellationToken = default);

    Task<PostDetails?> GetPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PostListItem>> GetPostsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PostListItem>> GetPostsByStepAsync(
        string zone,
        string step,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RoadmapZoneDto>> GetDotnetRoadmapAsync(
        CancellationToken cancellationToken = default);
}

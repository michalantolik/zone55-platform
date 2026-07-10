using BlogPlatform.App.Models;
using BlogPlatform.App.Models.LearnKit;
using BlogPlatform.App.Models.LearnKit.Articles;
using BlogPlatform.App.Models.LearnKit.Roadmap;
using BlogPlatform.Contracts.DotnetRoadmap;

namespace BlogPlatform.App.Services;

public interface IBlogApiClient
{
    Task<BlogHomeContent> GetHomeContentAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PostListItem>> GetPostsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PostListItem>> GetPostsByStepAsync(
        string zone,
        string step,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RoadmapZoneDto>> GetDotnetRoadmapAsync(
        CancellationToken cancellationToken = default);

    Task<LearnKitArticleDetails?> GetLearnKitArticleBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<LearnKitLearningPathDetails?> GetLearnKitLearningPathByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

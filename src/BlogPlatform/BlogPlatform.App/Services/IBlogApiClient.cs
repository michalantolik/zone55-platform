using BlogPlatform.App.Models.LearnKit.Articles;
using BlogPlatform.App.Models.LearnKit.Roadmap;

namespace BlogPlatform.App.Services;

public interface IBlogApiClient
{
    Task<LearnKitArticleDetails?> GetLearnKitArticleBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<LearnKitLearningPathDetails?> GetLearnKitLearningPathByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

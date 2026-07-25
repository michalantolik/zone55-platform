using Zone55.App.Models.LearnKit.Articles;
using Zone55.App.Models.LearnKit.Roadmap;

namespace Zone55.App.Services.LearnKit;

public interface ILearnKitApiClient
{
    Task<LearnKitArticleDetails?> GetLearnKitArticleBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<LearnKitLearningPathDetails?> GetLearnKitLearningPathByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

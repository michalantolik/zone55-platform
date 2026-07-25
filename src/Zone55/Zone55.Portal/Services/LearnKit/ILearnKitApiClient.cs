using Zone55.Portal.Models.LearnKit.Articles;
using Zone55.Portal.Models.LearnKit.Roadmap;

namespace Zone55.Portal.Services.LearnKit;

public interface ILearnKitApiClient
{
    Task<LearnKitArticleDetails?> GetLearnKitArticleBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<LearnKitLearningPathDetails?> GetLearnKitLearningPathByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
}

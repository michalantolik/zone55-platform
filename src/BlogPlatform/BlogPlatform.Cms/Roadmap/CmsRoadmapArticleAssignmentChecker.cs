using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Seeding;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Roadmap;

public sealed class CmsRoadmapArticleAssignmentChecker : IRoadmapArticleAssignmentChecker
{
    private readonly IContentService _contentService;

    public CmsRoadmapArticleAssignmentChecker(IContentService contentService)
    {
        _contentService = contentService;
    }

    public Task<bool> HasArticlesAssignedToZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken = default)
    {
        var result = GetRootArticles()
            .Any(article =>
                string.Equals(
                    article.GetValue<string>(BlogContentAliases.DotnetZone),
                    zoneKey,
                    StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(result);
    }

    public Task<bool> HasArticlesAssignedToStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken = default)
    {
        var result = GetRootArticles()
            .Any(article =>
                string.Equals(
                    article.GetValue<string>(BlogContentAliases.DotnetZone),
                    zoneKey,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    article.GetValue<string>(BlogContentAliases.DotnetZoneStep),
                    stepKey,
                    StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(result);
    }

    private IEnumerable<Umbraco.Cms.Core.Models.IContent> GetRootArticles()
    {
        return _contentService
            .GetRootContent()
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogArticle);
    }
}

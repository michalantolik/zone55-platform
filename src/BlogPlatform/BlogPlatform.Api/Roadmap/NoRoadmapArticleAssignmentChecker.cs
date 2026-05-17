using BlogPlatform.Application.Roadmap;

namespace BlogPlatform.Api.Roadmap;

public sealed class NoRoadmapArticleAssignmentChecker : IRoadmapArticleAssignmentChecker
{
    public Task<bool> HasArticlesAssignedToZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> HasArticlesAssignedToStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}

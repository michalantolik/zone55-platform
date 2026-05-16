namespace BlogPlatform.Application.Roadmap;

public interface IRoadmapArticleAssignmentChecker
{
    Task<bool> HasArticlesAssignedToZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken = default);

    Task<bool> HasArticlesAssignedToStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken = default);
}

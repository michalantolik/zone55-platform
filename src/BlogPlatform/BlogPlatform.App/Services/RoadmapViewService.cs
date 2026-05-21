using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public sealed class RoadmapViewService : IRoadmapViewService
{
    private readonly IBlogApiClient _blogApi;

    public RoadmapViewService(IBlogApiClient blogApi)
    {
        _blogApi = blogApi;
    }

    public async Task<IReadOnlyCollection<LearningPathLevel>> GetDotnetRoadmapAsync(
        IReadOnlyCollection<PostListItem> posts,
        CancellationToken cancellationToken = default)
    {
        var zones = await _blogApi.GetDotnetRoadmapAsync(cancellationToken);

        return zones
            .OrderBy(zone => zone.Order)
            .Select(zone => new LearningPathLevel(
                zone.Key,
                zone.Order,
                zone.Name,
                $"Follow the {zone.Name} learning path.",
                GetAccentClass(zone.Order),
                zone.Steps
                    .OrderBy(step => step.Order)
                    .Select(step => new LearningPathStep(
                        GlobalOrder: step.Order,
                        StepOrder: step.Order,
                        Key: step.Key,
                        Title: step.Name,
                        Icon: step.Icon,
                        Description: $"Learn {step.Name}.",
                        Difficulty: "Guided",
                        Keywords: [],
                        Posts: posts
                            .Where(post =>
                                string.Equals(post.DotnetZone, zone.Key, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(post.DotnetZoneStep, step.Key, StringComparison.OrdinalIgnoreCase))
                            .ToList()))
                    .ToList()))
            .ToList();
    }

    private static string GetAccentClass(int order)
    {
        return order switch
        {
            1 => "learning-path-accent-foundation",
            2 => "learning-path-accent-web",
            3 => "learning-path-accent-architecture",
            4 => "learning-path-accent-cloud",
            _ => "learning-path-accent-foundation"
        };
    }
}

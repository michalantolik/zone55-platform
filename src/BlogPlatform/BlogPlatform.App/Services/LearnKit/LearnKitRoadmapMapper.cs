using BlogPlatform.App.Models;
using BlogPlatform.App.Models.LearnKit.Roadmap;

namespace BlogPlatform.App.Services.LearnKit;

public static class LearnKitRoadmapMapper
{
    public static IReadOnlyCollection<LearningPathLevel> ToLearningPathLevels(
        LearnKitLearningPathDetails learningPath)
    {
        return learningPath.Zones
            .OrderBy(zone => zone.SortOrder)
            .Select(zone => new LearningPathLevel(
                Key: zone.Key,
                Number: zone.SortOrder,
                Title: zone.Title,
                Description: zone.Summary,
                AccentClass: GetAccentClass(zone.SortOrder),
                Steps: zone.Steps
                    .OrderBy(step => step.SortOrder)
                    .Select(step => new LearningPathStep(
                        GlobalOrder: step.SortOrder,
                        StepOrder: step.SortOrder,
                        Key: step.Key,
                        Title: step.Title,
                        Description: step.Summary,
                        Difficulty: "Guided",
                        Icon: string.Empty,
                        Keywords: [],
                        Posts: step.Articles
                            .OrderBy(article => article.SortOrder)
                            .Select(article => new PostListItem(
                                Slug: article.Slug,
                                Title: article.Title,
                                Summary: article.Summary,
                                Level: string.Empty,
                                Focus: string.Empty,
                                DotnetZone: zone.Key,
                                DotnetZoneStep: step.Key,
                                Order: article.SortOrder,
                                Tags: [],
                                PublishedDate: null))
                            .ToArray()))
                    .ToArray()))
            .ToArray();
    }

    private static string GetAccentClass(int sortOrder)
    {
        return sortOrder switch
        {
            1 => "learning-path-accent-foundation",
            2 => "learning-path-accent-web",
            3 => "learning-path-accent-architecture",
            4 => "learning-path-accent-cloud",
            _ => "learning-path-accent-foundation"
        };
    }
}

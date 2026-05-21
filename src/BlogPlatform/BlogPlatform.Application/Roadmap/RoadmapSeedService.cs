using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Roadmap;

public sealed class RoadmapSeedService : IRoadmapSeedService
{
    private readonly IDotnetRoadmapStore _roadmapStore;

    public RoadmapSeedService(IDotnetRoadmapStore roadmapStore)
    {
        _roadmapStore = roadmapStore;
    }

    public async Task SeedAsync(
        RoadmapSeedModel seed,
        CancellationToken cancellationToken = default)
    {
        var zones = seed.Zones
            .OrderBy(zone => zone.Order)
            .Select(zone => DotnetRoadmapZone.Create(
                zone.Key,
                zone.Name,
                zone.Order,
                zone.Steps
                    .OrderBy(step => step.Order)
                    .Select(step => DotnetRoadmapStep.Create(
                        step.Key,
                        step.Name,
                        step.Order,
                        step.Icon))))
            .ToList();

        if (zones.Count == 0)
        {
            return;
        }

        await _roadmapStore.SaveAsync(
            DotnetRoadmap.Create(zones),
            cancellationToken);
    }
}

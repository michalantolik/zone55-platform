using BlogPlatform.Application.Roadmap;
using BlogPlatform.Contracts.DotnetRoadmap;

namespace BlogPlatform.Api.Mapping;

internal static class RoadmapContractMapper
{
    public static RoadmapZoneDto ToDto(RoadmapZoneModel zone)
    {
        return new RoadmapZoneDto(
            zone.Key,
            zone.Name,
            zone.Order,
            zone.Steps
                .OrderBy(step => step.Order)
                .Select(ToDto)
                .ToList());
    }

    private static RoadmapStepDto ToDto(RoadmapStepModel step)
    {
        return new RoadmapStepDto(
            step.Key,
            step.Name,
            step.Order);
    }
}

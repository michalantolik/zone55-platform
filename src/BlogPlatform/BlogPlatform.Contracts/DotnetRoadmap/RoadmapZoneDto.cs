namespace BlogPlatform.Contracts.DotnetRoadmap;

public sealed record RoadmapZoneDto(
    string Key,
    string Name,
    int Order,
    IReadOnlyCollection<RoadmapStepDto> Steps);

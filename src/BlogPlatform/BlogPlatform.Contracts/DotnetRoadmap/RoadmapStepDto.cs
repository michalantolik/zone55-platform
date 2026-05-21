namespace BlogPlatform.Contracts.DotnetRoadmap;

public sealed record RoadmapStepDto(
    string Key,
    string Name,
    int Order,
    string Icon);

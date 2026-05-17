namespace BlogPlatform.Application.Roadmap;

public sealed record RoadmapSeedModel(
    IReadOnlyCollection<RoadmapSeedZoneModel> Zones);

public sealed record RoadmapSeedZoneModel(
    string Key,
    string Name,
    int Order,
    IReadOnlyCollection<RoadmapSeedStepModel> Steps);

public sealed record RoadmapSeedStepModel(
    string Key,
    string Name,
    int Order);

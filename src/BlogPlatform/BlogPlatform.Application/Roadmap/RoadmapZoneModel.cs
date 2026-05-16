namespace BlogPlatform.Application.Roadmap;

public sealed record RoadmapZoneModel(
    string Key,
    string Name,
    int Order,
    IReadOnlyCollection<RoadmapStepModel> Steps);

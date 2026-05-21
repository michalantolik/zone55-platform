namespace BlogPlatform.Application.Roadmap;

public sealed record RoadmapStepModel(
    string Key,
    string Name,
    int Order,
    string Icon);

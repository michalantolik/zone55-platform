namespace BlogPlatform.Infrastructure.Persistence;

public sealed class RoadmapStepEntity
{
    public int RoadmapStepId { get; set; }

    public int RoadmapZoneId { get; set; }

    public RoadmapZoneEntity Zone { get; set; } = default!;

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}

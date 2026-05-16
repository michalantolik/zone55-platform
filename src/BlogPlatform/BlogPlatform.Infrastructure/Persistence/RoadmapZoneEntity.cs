namespace BlogPlatform.Infrastructure.Persistence;

public sealed class RoadmapZoneEntity
{
    public int RoadmapZoneId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public ICollection<RoadmapStepEntity> Steps { get; } = [];
}

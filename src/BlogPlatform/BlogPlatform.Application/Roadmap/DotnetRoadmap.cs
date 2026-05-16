namespace BlogPlatform.Application.Roadmap;

public sealed class DotnetRoadmap
{
    public List<DotnetRoadmapZone> Zones { get; set; } = [];
}

public sealed class DotnetRoadmapZone
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<DotnetRoadmapStep> Steps { get; set; } = [];
}

public sealed class DotnetRoadmapStep
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }
}

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogSeedContent
{
    public List<BlogSeedRoadmapZone> RoadmapZones { get; set; } = [];
    public List<BlogSeedArticle> Articles { get; set; } = [];
}

public sealed class BlogSeedRoadmapZone
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<BlogSeedRoadmapStep> Steps { get; set; } = [];
}

public sealed class BlogSeedRoadmapStep
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Icon { get; set; } = "📘";
}

public sealed class BlogSeedArticle
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string DotnetZone { get; set; } = "foundation";
    public string DotnetZoneStep { get; set; } = "basic-syntax";
    public int Order { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<BlogSeedBlock> BodyBlocks { get; set; } = [];
}

public sealed class BlogSeedBlock
{
    public string Type { get; set; } = string.Empty;
    public int? Level { get; set; }
    public string? Text { get; set; }
    public string? Summary { get; set; }
    public string? Language { get; set; }
    public string? FileName { get; set; }
    public string? Code { get; set; }
    public string? Diagram { get; set; }
    public string? Title { get; set; }
    public bool? ShowDiagramTitleBar { get; set; }
    public string? Kind { get; set; }
}

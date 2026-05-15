namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeederOptions
{
    public bool Enabled { get; set; } = true;

    public string ContentFilePath { get; set; } = "Seeding/blog-content.seed.json";
}

namespace BlogPlatform.Cms.Seeding.Blocks;

public sealed record SeedBlock(
    string ElementTypeAlias,
    Dictionary<string, object?> Properties);

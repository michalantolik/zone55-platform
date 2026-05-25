using System.Text.Json;

namespace BlogPlatform.Cms.Seeding.Blocks;

public interface ISeedBlockSerializationStrategy
{
    string ElementTypeAlias { get; }

    string SeedType { get; }

    bool CanExport(string elementTypeAlias, JsonElement blockElement);

    bool CanImport(BlogSeedBlock block);

    BlogSeedBlock Export(JsonElement blockElement);

    SeedBlock Import(BlogSeedBlock block);

    void NormalizeForUmbraco(Dictionary<string, object?> block)
    {
    }
}

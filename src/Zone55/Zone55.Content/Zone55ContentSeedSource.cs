using LearnKit.Infrastructure.Seed;

namespace Zone55.Content;

internal sealed class Zone55ContentSeedSource : ILearnKitContentSeedSource
{
    private const string ResourceName =
        "Zone55.Content.Seed.Content.zone55-content.seed.json";

    public Stream OpenRead() =>
        typeof(Zone55ContentSeedSource).Assembly
            .GetManifestResourceStream(ResourceName)
        ?? throw new InvalidOperationException(
            $"Embedded content seed '{ResourceName}' was not found.");

    public string SourceVersion => "zone55-content.seed.v1";
}

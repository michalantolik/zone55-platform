using LearnKit.Infrastructure.Seed;

namespace Backend55.Content;

internal sealed class Backend55ContentSeedSource : ILearnKitContentSeedSource
{
    private const string ResourceName =
        "Backend55.Content.Seed.Content.backend55-content.seed.json";

    public Stream OpenRead() =>
        typeof(Backend55ContentSeedSource).Assembly
            .GetManifestResourceStream(ResourceName)
        ?? throw new InvalidOperationException(
            $"Embedded content seed '{ResourceName}' was not found.");

    public string SourceVersion => "backend55-content.seed.v1";
}

namespace BlogPlatform.Application.Roadmap;

public interface IRoadmapSeedService
{
    Task SeedAsync(
        RoadmapSeedModel seed,
        CancellationToken cancellationToken = default);
}

namespace BlogPlatform.Application.Roadmap;

public interface IDotnetRoadmapStore
{
    Task<DotnetRoadmap> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(
        DotnetRoadmap roadmap,
        CancellationToken cancellationToken = default);
}

namespace BlogPlatform.Application.Roadmap;

public interface IRoadmapCommandService
{
    Task<RoadmapOperationResult> CreateZoneAsync(
        string? name,
        string? requestedKey,
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> UpdateZoneAsync(
        string zoneKey,
        string? name,
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> DeleteZoneAsync(
        string zoneKey,
        bool hasAssignedArticles,
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> CreateStepAsync(
        string zoneKey,
        string? name,
        string? requestedKey,
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> UpdateStepAsync(
        string zoneKey,
        string stepKey,
        string? name,
        CancellationToken cancellationToken = default);

    Task<RoadmapOperationResult> DeleteStepAsync(
        string zoneKey,
        string stepKey,
        bool hasAssignedArticles,
        CancellationToken cancellationToken = default);
}

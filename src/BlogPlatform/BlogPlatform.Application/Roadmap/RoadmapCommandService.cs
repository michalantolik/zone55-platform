using BlogPlatform.Domain.ValueObjects;

namespace BlogPlatform.Application.Roadmap;

public sealed class RoadmapCommandService : IRoadmapCommandService
{
    private readonly IDotnetRoadmapStore _roadmapStore;
    private readonly IRoadmapArticleAssignmentChecker _assignmentChecker;

    public RoadmapCommandService(
        IDotnetRoadmapStore roadmapStore,
        IRoadmapArticleAssignmentChecker assignmentChecker)
    {
        _roadmapStore = roadmapStore;
        _assignmentChecker = assignmentChecker;
    }

    public async Task<RoadmapOperationResult> CreateZoneAsync(
        string? name,
        string? requestedKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNameRequired,
                "Zone name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);
        var key = CreateComparisonKey(requestedKey ?? name);

        if (roadmap.ContainsZone(key))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneKeyAlreadyExists,
                "Zone key already exists.");
        }

        roadmap.AddZone(name, requestedKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Zone created successfully.");
    }

    public async Task<RoadmapOperationResult> UpdateZoneAsync(
        string zoneKey,
        string? name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNameRequired,
                "Zone name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNotFound,
                "Zone not found.");
        }

        roadmap.RenameZone(zoneKey, name);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Zone updated successfully.");
    }

    public async Task<RoadmapOperationResult> DeleteZoneAsync(
        string zoneKey,
        CancellationToken cancellationToken = default)
    {
        if (await _assignmentChecker.HasArticlesAssignedToZoneAsync(zoneKey, cancellationToken))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneHasAssignedArticles,
                "Cannot delete zone because articles still use it.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNotFound,
                "Zone not found.");
        }

        roadmap.DeleteZone(zoneKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Zone deleted successfully.");
    }

    public async Task<RoadmapOperationResult> CreateStepAsync(
        string zoneKey,
        string? name,
        string? requestedKey,
        string? icon,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepNameRequired,
                "Step name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.ContainsZone(zoneKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.ZoneNotFound,
                "Zone not found.");
        }

        var key = CreateComparisonKey(requestedKey ?? name);

        if (roadmap.ContainsStep(key))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepKeyAlreadyExists,
                "Step key already exists.");
        }

        roadmap.AddStep(zoneKey, name, requestedKey, icon);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Step created successfully.");
    }

    public async Task<RoadmapOperationResult> UpdateStepAsync(
        string zoneKey,
        string stepKey,
        string? name,
        string? icon,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepNameRequired,
                "Step name is required.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepNotFound,
                "Step not found.");
        }

        roadmap.RenameStep(zoneKey, stepKey, name, icon);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Step updated successfully.");
    }

    public async Task<RoadmapOperationResult> DeleteStepAsync(
        string zoneKey,
        string stepKey,
        CancellationToken cancellationToken = default)
    {
        if (await _assignmentChecker.HasArticlesAssignedToStepAsync(zoneKey, stepKey, cancellationToken))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepHasAssignedArticles,
                "Cannot delete step because articles still use it.");
        }

        var roadmap = await _roadmapStore.GetAsync(cancellationToken);

        if (!roadmap.IsValidAssignment(zoneKey, stepKey))
        {
            return RoadmapOperationResult.Fail(
                RoadmapOperationError.StepNotFound,
                "Step not found.");
        }

        roadmap.DeleteStep(zoneKey, stepKey);

        await _roadmapStore.SaveAsync(roadmap, cancellationToken);

        return RoadmapOperationResult.Ok("Step deleted successfully.");
    }

    private static string CreateComparisonKey(string value)
    {
        return Slug.Create(value).Value;
    }
}

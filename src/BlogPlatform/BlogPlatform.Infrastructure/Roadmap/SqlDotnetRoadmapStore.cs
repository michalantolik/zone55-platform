using BlogPlatform.Application.Roadmap;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Roadmap;

public sealed class SqlDotnetRoadmapStore : IDotnetRoadmapStore
{
    private readonly BlogPlatformDbContext _dbContext;

    public SqlDotnetRoadmapStore(BlogPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DotnetRoadmap> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var zones = await _dbContext.RoadmapZones
            .AsNoTracking()
            .Include(zone => zone.Steps)
            .OrderBy(zone => zone.Order)
            .ToListAsync(cancellationToken);

        if (zones.Count == 0)
        {
            return DotnetRoadmap.Create([]);
        }

        return DotnetRoadmap.Create(
            zones.Select(zone => DotnetRoadmapZone.Create(
                zone.Key,
                zone.Name,
                zone.Order,
                zone.Steps
                    .OrderBy(step => step.Order)
                    .Select(step => DotnetRoadmapStep.Create(
                        step.Key,
                        step.Name,
                        step.Order,
                        step.Icon)))));
    }

    public async Task SaveAsync(
        DotnetRoadmap roadmap,
        CancellationToken cancellationToken = default)
    {
        var existingZones = await _dbContext.RoadmapZones
            .Include(zone => zone.Steps)
            .ToListAsync(cancellationToken);

        var requestedZoneKeys = roadmap.Zones
            .Select(zone => zone.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var zonesToRemove = existingZones
            .Where(zone => !requestedZoneKeys.Contains(zone.Key))
            .ToList();

        _dbContext.RoadmapZones.RemoveRange(zonesToRemove);

        foreach (var zone in roadmap.Zones.OrderBy(zone => zone.Order))
        {
            var zoneEntity = existingZones.FirstOrDefault(existing =>
                string.Equals(existing.Key, zone.Key, StringComparison.OrdinalIgnoreCase));

            if (zoneEntity is null)
            {
                zoneEntity = new RoadmapZoneEntity
                {
                    Key = zone.Key
                };

                _dbContext.RoadmapZones.Add(zoneEntity);
            }

            zoneEntity.Name = zone.Name;
            zoneEntity.Order = zone.Order;

            var requestedStepKeys = zone.Steps
                .Select(step => step.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var stepsToRemove = zoneEntity.Steps
                .Where(step => !requestedStepKeys.Contains(step.Key))
                .ToList();

            foreach (var stepToRemove in stepsToRemove)
            {
                zoneEntity.Steps.Remove(stepToRemove);
            }

            foreach (var step in zone.Steps.OrderBy(step => step.Order))
            {
                var stepEntity = zoneEntity.Steps.FirstOrDefault(existing =>
                    string.Equals(existing.Key, step.Key, StringComparison.OrdinalIgnoreCase));

                if (stepEntity is null)
                {
                    stepEntity = new RoadmapStepEntity
                    {
                        Key = step.Key
                    };

                    zoneEntity.Steps.Add(stepEntity);
                }

                stepEntity.Name = step.Name;
                stepEntity.Icon = step.Icon;
                stepEntity.Order = step.Order;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

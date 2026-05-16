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
            return DotnetRoadmapDefaults.Create();
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
                        step.Order)))));
    }

    public async Task SaveAsync(
        DotnetRoadmap roadmap,
        CancellationToken cancellationToken = default)
    {
        var existingZones = await _dbContext.RoadmapZones
            .Include(zone => zone.Steps)
            .ToListAsync(cancellationToken);

        _dbContext.RoadmapZones.RemoveRange(existingZones);

        var entities = roadmap.Zones
            .OrderBy(zone => zone.Order)
            .Select(zone =>
            {
                var zoneEntity = new RoadmapZoneEntity
                {
                    Key = zone.Key,
                    Name = zone.Name,
                    Order = zone.Order
                };

                foreach (var step in zone.Steps.OrderBy(step => step.Order))
                {
                    zoneEntity.Steps.Add(new RoadmapStepEntity
                    {
                        Key = step.Key,
                        Name = step.Name,
                        Order = step.Order
                    });
                }

                return zoneEntity;
            })
            .ToList();

        await _dbContext.RoadmapZones.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Roadmaps;

internal sealed class EfLearningPathManagementStore : ILearningPathManagementStore
{
    private readonly LearnKitDbContext _dbContext;

    public EfLearningPathManagementStore(LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LearningPathManagementDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningPaths
            .AsNoTracking()
            .Where(path => path.Key == key)
            .Select(path => new LearningPathManagementDetails(
                path.Id,
                path.Key,
                path.Title,
                path.Summary,
                path.Zones
                    .OrderBy(zone => zone.SortOrder)
                    .Select(zone => new LearningZoneManagementDetails(
                        zone.Id,
                        zone.Key,
                        zone.Title,
                        zone.Summary,
                        zone.SortOrder,
                        zone.Steps
                            .OrderBy(step => step.SortOrder)
                            .Select(step => new LearningStepManagementDetails(
                                step.Id,
                                step.Key,
                                step.Title,
                                step.Summary,
                                step.SortOrder))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<LearningPath?> GetTrackedPathByIdAsync(
        Guid learningPathId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningPaths
            .Include(path => path.Zones)
                .ThenInclude(zone => zone.Steps)
            .SingleOrDefaultAsync(path => path.Id == learningPathId, cancellationToken);
    }

    public Task<LearningZone?> GetTrackedZoneByIdAsync(
        Guid learningZoneId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningZones
            .Include(zone => zone.Steps)
                .ThenInclude(step => step.Articles)
            .SingleOrDefaultAsync(zone => zone.Id == learningZoneId, cancellationToken);
    }

    public Task<LearningStep?> GetTrackedStepByIdAsync(
        Guid learningStepId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningSteps
            .Include(step => step.Articles)
            .SingleOrDefaultAsync(step => step.Id == learningStepId, cancellationToken);
    }

    public Task<bool> ZoneKeyExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim();
        return _dbContext.LearningZones.AnyAsync(
            zone => zone.Key == normalizedKey,
            cancellationToken);
    }

    public Task<bool> StepKeyExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim();
        return _dbContext.LearningSteps.AnyAsync(
            step => step.Key == normalizedKey,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

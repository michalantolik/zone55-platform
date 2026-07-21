using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Infrastructure.Persistence;
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
                path.Zones
                    .OrderBy(zone => zone.SortOrder)
                    .Select(zone => new LearningZoneManagementDetails(
                        zone.Id,
                        zone.Key,
                        zone.Title,
                        zone.SortOrder,
                        zone.Steps
                            .OrderBy(step => step.SortOrder)
                            .Select(step => new LearningStepManagementDetails(
                                step.Id,
                                step.Key,
                                step.Title,
                                step.SortOrder))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

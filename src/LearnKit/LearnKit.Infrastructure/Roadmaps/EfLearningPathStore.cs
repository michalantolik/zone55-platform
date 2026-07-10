using LearnKit.Application.Roadmaps.Public.Contracts;
using LearnKit.Application.Roadmaps.Public.Models;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Roadmaps;

/// <summary>
/// Entity Framework implementation of learning path read access.
/// </summary>
internal sealed class EfLearningPathStore : ILearningPathStore
{
    private readonly LearnKitDbContext _dbContext;

    public EfLearningPathStore(LearnKitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LearningPathDetails?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return await _dbContext.LearningPaths
            .AsNoTracking()
            .Where(path => path.Key == key)
            .Select(path => new LearningPathDetails
            {
                Key = path.Key,
                Title = path.Title,
                Summary = path.Summary,
                Zones = path.Zones
                    .OrderBy(zone => zone.SortOrder)
                    .Select(zone => new LearningZoneDetails
                    {
                        Key = zone.Key,
                        Title = zone.Title,
                        Summary = zone.Summary,
                        SortOrder = zone.SortOrder,
                        Steps = zone.Steps
                            .OrderBy(step => step.SortOrder)
                            .Select(step => new LearningStepDetails
                            {
                                Key = step.Key,
                                Title = step.Title,
                                Summary = step.Summary,
                                SortOrder = step.SortOrder,
                                Articles = step.Articles
                                    .Select(article => new ArticleSummary
                                    {
                                        Slug = article.Slug,
                                        Title = article.Title,
                                        Summary = article.Summary,
                                        Status = article.Status.ToString(),
                                        SortOrder = article.SortOrder
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}

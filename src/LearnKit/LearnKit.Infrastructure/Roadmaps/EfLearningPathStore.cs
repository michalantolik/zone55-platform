using LearnKit.Application.Roadmaps.Public.Contracts;
using LearnKit.Application.Roadmaps.Public.Models;
using LearnKit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearnKit.Infrastructure.Roadmaps;

internal sealed class EfLearningPathStore(LearnKitDbContext dbContext) : ILearningPathStore
{
    public Task<LearningPathDetails?> GetFirstAsync(CancellationToken cancellationToken = default)
    {
        return Project(dbContext.LearningPaths.AsNoTracking())
            .OrderBy(path => path.SortOrder)
            .ThenBy(path => path.Key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<LearningPathDetails?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Project(dbContext.LearningPaths.AsNoTracking().Where(path => path.Key == key))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<LearningPathDetails> Project(IQueryable<LearnKit.Domain.Roadmaps.LearningPath> paths)
    {
        return paths.Select(path => new LearningPathDetails
        {
            Key = path.Key, Title = path.Title, Summary = path.Summary, SortOrder = path.SortOrder,
            Zones = path.Zones.OrderBy(zone => zone.SortOrder).Select(zone => new LearningZoneDetails
            {
                Key = zone.Key, Title = zone.Title, Summary = zone.Summary, SortOrder = zone.SortOrder,
                Steps = zone.Steps.OrderBy(step => step.SortOrder).Select(step => new LearningStepDetails
                {
                    Key = step.Key, Title = step.Title, Summary = step.Summary, SortOrder = step.SortOrder,
                    Articles = step.Articles.Select(article => new ArticleSummary
                    { Slug = article.Slug, Title = article.Title, Summary = article.Summary, Status = article.Status.ToString(), SortOrder = article.SortOrder }).ToList()
                }).ToList()
            }).ToList()
        });
    }
}

using LearnKit.Application.Articles.Admin.Contracts;
namespace LearnKit.Application.Articles.Admin.Commands.ReorderArticles;
public sealed class ReorderArticlesHandler
{
    private readonly IArticleManagementStore _store;
    public ReorderArticlesHandler(IArticleManagementStore store) => _store = store;
    public async Task<bool> HandleAsync(ReorderArticlesCommand command, CancellationToken cancellationToken = default)
    {
        var articles = await _store.GetTrackedByStepIdAsync(command.LearningStepId, cancellationToken);
        if (articles.Count != command.OrderedArticleIds.Count ||
            command.OrderedArticleIds.Distinct().Count() != command.OrderedArticleIds.Count ||
            command.OrderedArticleIds.Any(id => articles.All(article => article.Id != id)))
        {
            return false;
        }
        var order = 1;
        foreach (var id in command.OrderedArticleIds)
            articles.Single(article => article.Id == id).ChangeSortOrder(order++);
        await _store.SaveChangesAsync(cancellationToken);
        return true;
    }
}

using LearnKit.Application.Articles.Admin.Contracts;
namespace LearnKit.Application.Articles.Admin.Commands.DeleteArticle;
public sealed class DeleteArticleHandler
{
    private readonly IArticleManagementStore _store;
    public DeleteArticleHandler(IArticleManagementStore store) => _store = store;
    public async Task<bool> HandleAsync(DeleteArticleCommand command, CancellationToken cancellationToken = default)
    {
        var article = await _store.GetTrackedByIdAsync(command.ArticleId, cancellationToken);
        if (article is null) return false;
        _store.Remove(article);
        await _store.SaveChangesAsync(cancellationToken);
        return true;
    }
}

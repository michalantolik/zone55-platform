using LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.UnpublishArticle;

public sealed class UnpublishArticleHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldMoveToDraftAndSave_WhenArticleExists()
    {
        var article = new Article(
            Guid.NewGuid(),
            "clean-architecture",
            "Clean Architecture",
            1);

        article.Publish();

        var store = new ArticleManagementStoreStub(article);
        var handler = new UnpublishArticleHandler(store);
        var command = new UnpublishArticleCommand(article.Id);

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        Assert.False(article.IsPublished);
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWithoutSaving_WhenArticleDoesNotExist()
    {
        var store = new ArticleManagementStoreStub(null);
        var handler = new UnpublishArticleHandler(store);
        var command = new UnpublishArticleCommand(Guid.NewGuid());

        var result = await handler.HandleAsync(command);

        Assert.False(result);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    private sealed class ArticleManagementStoreStub(
        Article? article) : IArticleManagementStore
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<IReadOnlyCollection<ArticleManagementListItem>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ArticleManagementDetails?> GetByIdAsync(
            Guid articleId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Article?> GetTrackedByIdAsync(
            Guid articleId,
            CancellationToken cancellationToken = default)
        {
            var result = article?.Id == articleId
                ? article
                : null;

            return Task.FromResult(result);
        }

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}

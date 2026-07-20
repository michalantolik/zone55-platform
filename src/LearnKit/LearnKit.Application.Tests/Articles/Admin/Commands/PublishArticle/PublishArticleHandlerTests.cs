using LearnKit.Application.Articles.Admin.Commands.PublishArticle;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.PublishArticle;

public sealed class PublishArticleHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPublishAndSave_WhenArticleExists()
    {
        var article = new Article(
            Guid.NewGuid(),
            "clean-architecture",
            "Clean Architecture",
            1);

        var store = new ArticleManagementStoreStub(article);
        var handler = new PublishArticleHandler(store);
        var command = new PublishArticleCommand(article.Id);

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        Assert.True(article.IsPublished);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWithoutSaving_WhenArticleDoesNotExist()
    {
        var store = new ArticleManagementStoreStub(null);
        var handler = new PublishArticleHandler(store);
        var command = new PublishArticleCommand(Guid.NewGuid());

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

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}

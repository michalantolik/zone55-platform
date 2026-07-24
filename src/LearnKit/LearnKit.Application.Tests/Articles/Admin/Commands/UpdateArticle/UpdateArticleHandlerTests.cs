using LearnKit.Application.Articles.Admin.Commands.UpdateArticle;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.UpdateArticle;

public sealed class UpdateArticleHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateAndSave_WhenArticleExists()
    {
        var article = new Article(
            Guid.NewGuid(),
            "old-slug",
            "Old title",
            1,
            "Old summary");

        var store = new ArticleManagementStoreStub(article);
        var handler = new UpdateArticleHandler(store);
        var newLearningStepId = Guid.NewGuid();
        var command = new UpdateArticleCommand(
            article.Id,
            newLearningStepId,
            "new-slug",
            "New title",
            "New summary",
            3);

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        Assert.Equal(newLearningStepId, article.LearningStepId);
        Assert.Equal("new-slug", article.Slug);
        Assert.Equal("New title", article.Title);
        Assert.Equal("New summary", article.Summary);
        Assert.Equal(3, article.SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWithoutSaving_WhenArticleDoesNotExist()
    {
        var store = new ArticleManagementStoreStub(null);
        var handler = new UpdateArticleHandler(store);
        var command = new UpdateArticleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "new-slug",
            "New title",
            "New summary",
            1);

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

        public Task<IReadOnlyCollection<Article>> GetTrackedByStepIdAsync(
            Guid learningStepId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void Remove(Article article)
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

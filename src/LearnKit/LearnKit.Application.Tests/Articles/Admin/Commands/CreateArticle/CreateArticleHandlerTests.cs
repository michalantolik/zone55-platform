using LearnKit.Application.Articles.Admin.Commands.CreateArticle;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.CreateArticle;

public sealed class CreateArticleHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateDraftArticleAndSave_WhenArgumentsAreValid()
    {
        var learningStepId = Guid.NewGuid();
        var store = new ArticleManagementStoreStub();
        var handler = new CreateArticleHandler(store);
        var command = new CreateArticleCommand(
            learningStepId,
            "clean-architecture",
            "Clean Architecture",
            "Article summary",
            2);

        var articleId = await handler.HandleAsync(command);

        var article = Assert.IsType<Article>(store.AddedArticle);
        Assert.Equal(article.Id, articleId);
        Assert.Equal(learningStepId, article.LearningStepId);
        Assert.Equal("clean-architecture", article.Slug);
        Assert.Equal("Clean Architecture", article.Title);
        Assert.Equal("Article summary", article.Summary);
        Assert.Equal(2, article.SortOrder);
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Equal(1, store.AddCallCount);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotAddOrSave_WhenArgumentsAreInvalid()
    {
        var store = new ArticleManagementStoreStub();
        var handler = new CreateArticleHandler(store);
        var command = new CreateArticleCommand(
            Guid.NewGuid(),
            string.Empty,
            "Clean Architecture",
            null,
            1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(command));

        Assert.Null(store.AddedArticle);
        Assert.Equal(0, store.AddCallCount);
        Assert.Equal(0, store.SaveChangesCallCount);
    }


    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateSlugWithoutSaving()
    {
        var store = new ArticleManagementStoreStub { SlugExists = true };
        var handler = new CreateArticleHandler(store);

        await Assert.ThrowsAsync<LearnKit.Application.Articles.Admin.ArticleSlugConflictException>(
            () => handler.HandleAsync(new CreateArticleCommand(
                Guid.NewGuid(),
                "clean-architecture",
                "Clean Architecture",
                null,
                1)));

        Assert.Null(store.AddedArticle);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    private sealed class ArticleManagementStoreStub : IArticleManagementStore
    {
        public Article? AddedArticle { get; private set; }

        public int AddCallCount { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public bool SlugExists { get; init; }

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
            throw new NotSupportedException();
        }

        public Task<bool> SlugExistsAsync(
            string slug,
            Guid? excludingArticleId = null,
            CancellationToken cancellationToken = default) => Task.FromResult(SlugExists);

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default)
        {
            AddedArticle = article;
            AddCallCount++;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}

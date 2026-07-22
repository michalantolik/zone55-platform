using LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.CreateArticleBlock;

public sealed class CreateArticleBlockHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldAddBlockAndSave_WhenArticleExists()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var store = new ArticleManagementStoreStub(article);
        var handler = new CreateArticleBlockHandler(store);

        var blockId = await handler.HandleAsync(new CreateArticleBlockCommand(
            article.Id,
            "Markdown",
            1,
            "{\"markdown\":\"Content\"}"));

        var block = Assert.Single(article.Blocks);
        Assert.Equal(block.Id, blockId);
        Assert.Equal(ArticleBlockType.Markdown, block.Type);
        Assert.Equal(1, block.SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectInvalidContentWithoutSaving()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var store = new ArticleManagementStoreStub(article);
        var handler = new CreateArticleBlockHandler(store);

        await Assert.ThrowsAsync<ArticleBlockContentValidationException>(
            () => handler.HandleAsync(new CreateArticleBlockCommand(
                article.Id,
                "Code",
                1,
                "{\"language\":\"csharp\"}")));

        Assert.Empty(article.Blocks);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNullWithoutSaving_WhenArticleDoesNotExist()
    {
        var store = new ArticleManagementStoreStub(null);
        var handler = new CreateArticleBlockHandler(store);

        var blockId = await handler.HandleAsync(new CreateArticleBlockCommand(
            Guid.NewGuid(), "Markdown", 1, "{}"));

        Assert.Null(blockId);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    private sealed class ArticleManagementStoreStub : IArticleManagementStore
    {
        private readonly Article? _article;

        public ArticleManagementStoreStub(Article? article)
        {
            _article = article;
        }

        public int SaveChangesCallCount { get; private set; }

        public Task<IReadOnlyCollection<ArticleManagementListItem>> GetAllAsync(
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<ArticleManagementDetails?> GetByIdAsync(
            Guid articleId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<Article?> GetTrackedByIdAsync(
            Guid articleId,
            CancellationToken cancellationToken = default) => Task.FromResult(_article);

        public Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

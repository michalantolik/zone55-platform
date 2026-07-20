using LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.ReorderArticleBlocks;

public sealed class ReorderArticleBlocksHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldApplyCompleteOrderAndSave_WhenArticleExists()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var first = new ArticleBlock(ArticleBlockType.Markdown, 1, "{}");
        var second = new ArticleBlock(ArticleBlockType.Code, 2, "{}");
        article.AddBlock(first);
        article.AddBlock(second);
        var store = new ArticleManagementStoreStub(article);
        var handler = new ReorderArticleBlocksHandler(store);

        var reordered = await handler.HandleAsync(new ReorderArticleBlocksCommand(
            article.Id,
            new[] { second.Id, first.Id }));

        Assert.True(reordered);
        Assert.Equal(new[] { second.Id, first.Id }, article.Blocks.Select(block => block.Id));
        Assert.Equal(new[] { 1, 2 }, article.Blocks.Select(block => block.SortOrder));
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectIncompleteOrderWithoutSaving()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var first = new ArticleBlock(ArticleBlockType.Markdown, 1, "{}");
        var second = new ArticleBlock(ArticleBlockType.Code, 2, "{}");
        article.AddBlock(first);
        article.AddBlock(second);
        var store = new ArticleManagementStoreStub(article);
        var handler = new ReorderArticleBlocksHandler(store);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(
            new ReorderArticleBlocksCommand(article.Id, new[] { first.Id })));

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

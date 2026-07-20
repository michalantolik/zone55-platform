using LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.UpdateArticleBlock;

public sealed class UpdateArticleBlockHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateBlockAndSave_WhenBlockExists()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var block = new ArticleBlock(ArticleBlockType.Markdown, 1, "{}");
        article.AddBlock(block);
        var store = new ArticleManagementStoreStub(article);
        var handler = new UpdateArticleBlockHandler(store);

        var updated = await handler.HandleAsync(new UpdateArticleBlockCommand(
            article.Id, block.Id, "Code", "{\"code\":\"dotnet test\"}"));

        Assert.True(updated);
        Assert.Equal(ArticleBlockType.Code, block.Type);
        Assert.Equal("{\"code\":\"dotnet test\"}", block.ContentJson);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWithoutSaving_WhenBlockDoesNotExist()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var store = new ArticleManagementStoreStub(article);
        var handler = new UpdateArticleBlockHandler(store);

        var updated = await handler.HandleAsync(new UpdateArticleBlockCommand(
            article.Id, Guid.NewGuid(), "Code", "{}"));

        Assert.False(updated);
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

using LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;
using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Application.Articles.Admin.Models;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Admin.Commands.DeleteArticleBlock;

public sealed class DeleteArticleBlockHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldDeleteBlockNormalizeOrderAndSave_WhenBlockExists()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var first = new ArticleBlock(ArticleBlockType.Markdown, 1, "{\"markdown\":\"Content\"}");
        var second = new ArticleBlock(ArticleBlockType.Code, 2, "{\"code\":\"dotnet test\"}");
        article.AddBlock(first);
        article.AddBlock(second);
        var store = new ArticleManagementStoreStub(article);
        var handler = new DeleteArticleBlockHandler(store);

        var deleted = await handler.HandleAsync(
            new DeleteArticleBlockCommand(article.Id, first.Id));

        Assert.True(deleted);
        var remaining = Assert.Single(article.Blocks);
        Assert.Equal(second.Id, remaining.Id);
        Assert.Equal(1, remaining.SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWithoutSaving_WhenBlockDoesNotExist()
    {
        var article = new Article(Guid.NewGuid(), "article", "Article", 1);
        var store = new ArticleManagementStoreStub(article);
        var handler = new DeleteArticleBlockHandler(store);

        var deleted = await handler.HandleAsync(
            new DeleteArticleBlockCommand(article.Id, Guid.NewGuid()));

        Assert.False(deleted);
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

using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;

/// <summary>
/// Handles requests to add article blocks.
/// </summary>
public sealed class CreateArticleBlockHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public CreateArticleBlockHandler(IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    public async Task<Guid?> HandleAsync(
        CreateArticleBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return null;
        }

        var type = ParseType(command.Type);
        var block = new ArticleBlock(type, command.SortOrder, command.ContentJson);

        article.AddBlock(block);
        await _articleManagementStore.SaveChangesAsync(cancellationToken);

        return block.Id;
    }

    private static ArticleBlockType ParseType(string type)
    {
        if (!Enum.TryParse<ArticleBlockType>(type, true, out var parsedType)
            || !Enum.IsDefined(parsedType))
        {
            throw new ArgumentException("Article block type is invalid.", nameof(type));
        }

        return parsedType;
    }
}

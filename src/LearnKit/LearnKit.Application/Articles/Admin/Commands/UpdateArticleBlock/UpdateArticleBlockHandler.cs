using LearnKit.Application.Articles.Admin.Contracts;
using LearnKit.Domain.Articles;

namespace LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;

/// <summary>
/// Handles requests to update article blocks.
/// </summary>
public sealed class UpdateArticleBlockHandler
{
    private readonly IArticleManagementStore _articleManagementStore;

    public UpdateArticleBlockHandler(IArticleManagementStore articleManagementStore)
    {
        _articleManagementStore = articleManagementStore;
    }

    public async Task<bool> HandleAsync(
        UpdateArticleBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleManagementStore.GetTrackedByIdAsync(
            command.ArticleId,
            cancellationToken);

        if (article is null)
        {
            return false;
        }

        var type = ParseType(command.Type);

        if (!article.UpdateBlock(command.BlockId, type, command.ContentJson))
        {
            return false;
        }

        await _articleManagementStore.SaveChangesAsync(cancellationToken);
        return true;
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

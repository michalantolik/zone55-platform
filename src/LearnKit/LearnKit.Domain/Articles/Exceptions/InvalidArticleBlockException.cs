using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Domain.Articles.Exceptions;

/// <summary>
/// Describes content that does not match the selected article block type.
/// </summary>
public sealed class InvalidArticleBlockException : ArgumentException
{
    public InvalidArticleBlockException(
        ArticleBlockType blockType,
        IReadOnlyCollection<string> errors)
        : base($"Content for the {blockType} article block is invalid.", "contentJson")
    {
        BlockType = blockType;
        Errors = errors;
    }

    public ArticleBlockType BlockType { get; }

    public IReadOnlyCollection<string> Errors { get; }
}

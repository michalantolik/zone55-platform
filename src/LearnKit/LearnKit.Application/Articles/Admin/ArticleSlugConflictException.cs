namespace LearnKit.Application.Articles.Admin;

/// <summary>
/// Raised when an article slug is already used by another article.
/// </summary>
public sealed class ArticleSlugConflictException : Exception
{
    public ArticleSlugConflictException(string slug)
        : base($"The article slug '{slug}' is already in use.")
    {
        Slug = slug;
    }

    public string Slug { get; }
}

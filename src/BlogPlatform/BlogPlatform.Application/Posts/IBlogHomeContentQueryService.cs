namespace BlogPlatform.Application.Posts;

public interface IBlogHomeContentQueryService
{
    Task<BlogHomeContent> GetHomeContentAsync(
        string? categorySlug,
        CancellationToken cancellationToken);
}

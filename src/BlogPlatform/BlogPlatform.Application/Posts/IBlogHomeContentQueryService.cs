using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.Application.Posts;

public interface IBlogHomeContentQueryService
{
    Task<BlogHomeContentDto> GetHomeContentAsync(
        string? categorySlug,
        CancellationToken cancellationToken);
}

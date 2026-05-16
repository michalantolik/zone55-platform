namespace BlogPlatform.Application.Posts;

public interface IBlogHomeContentQueryService
{
    Task<BlogHomeContent> GetHomeContentAsync(
        CancellationToken cancellationToken);
}

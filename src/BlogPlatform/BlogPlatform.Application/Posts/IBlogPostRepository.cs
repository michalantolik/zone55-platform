using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Posts;

public interface IBlogPostRepository
{
    Task<IReadOnlyCollection<Post>> GetPostsAsync(
        CancellationToken cancellationToken);
}

using BlogPlatform.Application.Posts;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IBlogPostQueryService _posts;

    public PostsController(IBlogPostQueryService posts)
    {
        _posts = posts;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PostListItemDto>>> GetPosts(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(category, cancellationToken);

        return Ok(posts);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailsDto>> GetPostBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var post = await _posts.GetPostBySlugAsync(slug, cancellationToken);

        return post is null
            ? NotFound()
            : Ok(post);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await _posts.GetCategoriesAsync(cancellationToken);

        return Ok(categories);
    }
}

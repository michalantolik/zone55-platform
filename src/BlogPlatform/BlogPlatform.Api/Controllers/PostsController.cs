using BlogPlatform.Application.Posts;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IBlogPostQueryService _posts;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IBlogPostQueryService posts,
        ILogger<PostsController> logger)
    {
        _posts = posts;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PostListItemDto>>> GetPosts(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(category, cancellationToken);

        return Ok(posts);
    }

    [HttpGet("home")]
    public async Task<ActionResult<BlogHomeContentDto>> GetHomeContent(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API loading home content. Category slug: {CategorySlug}",
            category);

        var categoriesTask = _posts.GetCategoriesAsync(cancellationToken);
        var postsTask = _posts.GetPublishedPostsAsync(category, cancellationToken);

        await Task.WhenAll(categoriesTask, postsTask);

        var result = new BlogHomeContentDto(
            await categoriesTask,
            await postsTask);

        _logger.LogInformation(
            "API loaded home content. Categories: {CategoryCount}. Posts: {PostCount}",
            result.Categories.Count,
            result.Posts.Count);

        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await _posts.GetCategoriesAsync(cancellationToken);

        return Ok(categories);
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
}

public sealed record BlogHomeContentDto(
    IReadOnlyCollection<CategoryDto> Categories,
    IReadOnlyCollection<PostListItemDto> Posts);

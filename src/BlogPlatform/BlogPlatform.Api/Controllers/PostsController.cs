using BlogPlatform.Api.Mapping;
using BlogPlatform.Application.Posts;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly IBlogPostQueryService _posts;
    private readonly IBlogHomeContentQueryService _homeContent;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IBlogPostQueryService posts,
        IBlogHomeContentQueryService homeContent,
        ILogger<PostsController> logger)
    {
        _posts = posts;
        _homeContent = homeContent;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PostListItemDto>>> GetPosts(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(category, cancellationToken);

        return Ok(posts.Select(PostContractMapper.ToDto).ToList());
    }

    [HttpGet("home")]
    public async Task<ActionResult<BlogHomeContentDto>> GetHomeContent(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API loading home content. Category slug: {CategorySlug}",
            category);

        var result = await _homeContent.GetHomeContentAsync(
            category,
            cancellationToken);

        var dto = new BlogHomeContentDto(
            result.Categories.Select(PostContractMapper.ToDto).ToList(),
            result.Posts.Select(PostContractMapper.ToDto).ToList());

        _logger.LogInformation(
            "API loaded home content. Categories: {CategoryCount}. Posts: {PostCount}",
            dto.Categories.Count,
            dto.Posts.Count);

        return Ok(dto);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await _posts.GetCategoriesAsync(cancellationToken);

        return Ok(categories.Select(PostContractMapper.ToDto).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailsDto>> GetPostBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var post = await _posts.GetPostBySlugAsync(slug, cancellationToken);

        return post is null
            ? NotFound()
            : Ok(PostContractMapper.ToDto(post));
    }
}

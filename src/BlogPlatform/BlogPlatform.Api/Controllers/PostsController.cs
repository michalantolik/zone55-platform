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

        return Ok(posts.Select(ToDto).ToList());
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
            result.Categories.Select(ToDto).ToList(),
            result.Posts.Select(ToDto).ToList());

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

        return Ok(categories.Select(ToDto).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailsDto>> GetPostBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var post = await _posts.GetPostBySlugAsync(slug, cancellationToken);

        return post is null
            ? NotFound()
            : Ok(ToDto(post));
    }

    private static CategoryDto ToDto(CategorySummary category)
    {
        return new CategoryDto(
            category.Slug,
            category.Name,
            category.Count);
    }

    private static PostListItemDto ToDto(PostListItem post)
    {
        return new PostListItemDto(
            post.Slug,
            post.Title,
            post.Summary,
            post.Category,
            post.CategorySlug,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate);
    }

    private static PostDetailsDto ToDto(PostDetails post)
    {
        return new PostDetailsDto(
            post.Slug,
            post.Title,
            post.Summary,
            post.Category,
            post.CategorySlug,
            post.Level,
            post.Focus,
            post.DotnetZone,
            post.DotnetZoneStep,
            post.Tags,
            post.PublishedDate,
            post.BodyHtml);
    }
}

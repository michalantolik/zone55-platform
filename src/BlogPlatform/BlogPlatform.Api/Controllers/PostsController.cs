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
        CancellationToken cancellationToken)
    {
        var posts = await _posts.GetPublishedPostsAsync(
            GetPublishedPostsQuery.All,
            cancellationToken);

        return Ok(posts.Select(PostContractMapper.ToDto).ToList());
    }

    [HttpGet("by-step")]
    public async Task<ActionResult<IReadOnlyCollection<PostListItemDto>>> GetPostsByStep(
        [FromQuery] string? zone,
        [FromQuery] string? step,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API loading posts by step. Zone: {Zone}. Step: {Step}",
            zone,
            step);

        var posts = await _posts.GetPublishedPostsForStepAsync(
            new GetPublishedPostsForStepQuery(zone, step),
            cancellationToken);

        _logger.LogInformation(
            "API loaded posts by step. Zone: {Zone}. Step: {Step}. Count: {Count}",
            zone,
            step,
            posts.Count);

        return Ok(posts.Select(PostContractMapper.ToDto).ToList());
    }

    [HttpGet("home")]
    public async Task<ActionResult<BlogHomeContentDto>> GetHomeContent(
        CancellationToken cancellationToken)
    {
        var result = await _homeContent.GetHomeContentAsync(cancellationToken);

        var dto = new BlogHomeContentDto(
            result.Posts.Select(PostContractMapper.ToDto).ToList());

        return Ok(dto);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailsDto>> GetPostBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var post = await _posts.GetPostBySlugAsync(
            slug,
            cancellationToken);

        return post is null
            ? NotFound()
            : Ok(PostContractMapper.ToDto(post));
    }
}

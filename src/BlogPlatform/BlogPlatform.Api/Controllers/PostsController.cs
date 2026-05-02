using Microsoft.AspNetCore.Mvc;
using BlogPlatform.Application.Posts;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> Get()
    {
        var posts = new List<PostDto>
        {
            new PostDto
            {
                Id = 1,
                Title = "First post",
                Content = "Hello from .NET API"
            }
        };

        return Ok(posts);
    }
}

using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Cms.Controllers;

public sealed class BlogAdminController : Controller
{
    [HttpGet("/blog-admin")]
    public IActionResult Index()
    {
        return View("~/Views/BlogAdmin/Index.cshtml");
    }
}

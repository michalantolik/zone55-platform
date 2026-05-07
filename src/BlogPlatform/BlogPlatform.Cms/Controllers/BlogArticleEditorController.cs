using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Cms.Controllers;

public sealed class BlogArticleEditorController : Controller
{
    private readonly IConfiguration _configuration;

    public BlogArticleEditorController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("/blog-admin/article-editor")]
    public IActionResult Index()
    {
        var previewUrl = _configuration["BlogPreview:AppPreviewUrl"]
            ?? "https://localhost:5002/preview/article";

        ViewData["PreviewUrl"] = previewUrl;

        return View("~/Views/BlogArticleEditor/Index.cshtml");
    }
}

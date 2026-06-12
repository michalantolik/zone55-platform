using BlogPlatform.Cms.Security;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Cms.Controllers;

[UmbracoBackOfficeOnly]
public sealed class BlogArticleEditorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlogArticleEditorController> _logger;

    public BlogArticleEditorController(
        IConfiguration configuration,
        ILogger<BlogArticleEditorController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("/blog-admin/article-editor")]
    public IActionResult Index()
    {
        var previewUrl = _configuration["BlogPreview:AppPreviewUrl"]
            ?? "https://localhost:5002/preview/article";

        _logger.LogInformation(
            "LIVE_PREVIEW CMS editor opened. PreviewUrl={PreviewUrl}",
            previewUrl);

        ViewData["PreviewUrl"] = previewUrl;

        return View("~/Views/BlogArticleEditor/Index.cshtml");
    }
}

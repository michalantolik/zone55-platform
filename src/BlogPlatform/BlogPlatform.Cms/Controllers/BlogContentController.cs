using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BlogPlatform.Cms.Seeding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Controllers;

[ApiController]
[Route("api/blog-content")]
public sealed class BlogContentController : ControllerBase
{
    private const string ArticlesCacheKey = "cms-blog-articles";

    private readonly IContentService _contentService;
    private readonly ILogger<BlogContentController> _logger;
    private readonly IMemoryCache _cache;

    public BlogContentController(
        IContentService contentService,
        ILogger<BlogContentController> logger,
        IMemoryCache cache)
    {
        _contentService = contentService;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyCollection<CmsPostDetailsDto>>> GetArticles(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(ArticlesCacheKey, out IReadOnlyCollection<CmsPostDetailsDto>? cachedArticles) &&
            cachedArticles is not null)
        {
            return Ok(cachedArticles);
        }

        var rootContent = _contentService.GetRootContent().ToList();

        var categories = rootContent
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogCategory)
            .ToDictionary(
                content => content.Key,
                content => new CmsCategoryDto(
                    GetString(content, BlogContentAliases.Slug) ?? CreateSlug(content.Name ?? "uncategorized"),
                    GetString(content, BlogContentAliases.Title) ?? content.Name ?? "Uncategorized"));

        var posts = rootContent
            .Where(content => content.ContentType.Alias == BlogContentAliases.BlogArticle)
            .Select(content => MapPost(content, categories))
            .OrderByDescending(post => post.PublishedDate)
            .ToList();

        _cache.Set(
            ArticlesCacheKey,
            posts,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(3),
                Priority = CacheItemPriority.High
            });

        return Ok(posts);
    }

    private static CmsPostDetailsDto MapPost(
        IContent content,
        IReadOnlyDictionary<Guid, CmsCategoryDto> categories)
    {
        var category = ResolveCategory(content, categories);

        var tags = GetString(content, BlogContentAliases.Tags)?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray() ?? [];

        var title = GetString(content, BlogContentAliases.Title)
            ?? content.Name
            ?? "Untitled";

        return new CmsPostDetailsDto(
            GetString(content, BlogContentAliases.Slug) ?? CreateSlug(title),
            title,
            GetString(content, BlogContentAliases.Summary) ?? string.Empty,
            category.Name,
            category.Slug,
            GetString(content, BlogContentAliases.Level) ?? "Intermediate",
            GetString(content, BlogContentAliases.Focus) ?? "Practical",
            tags,
            GetDateTimeOffset(content, BlogContentAliases.PublishedDate),
            GetString(content, BlogContentAliases.BodyBlocks) ?? string.Empty);
    }

    private static string RenderBodyHtml(IContent content)
    {
        var rawJson = content.GetValue<string>(BlogContentAliases.BodyBlocks);

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(rawJson);

        if (!document.RootElement.TryGetProperty("layout", out var layout) ||
            !layout.TryGetProperty("Umbraco.BlockList", out var layoutItems) ||
            !document.RootElement.TryGetProperty("contentData", out var contentData))
        {
            return string.Empty;
        }

        var blocksByUdi = contentData
            .EnumerateArray()
            .Where(x => x.TryGetProperty("udi", out _))
            .ToDictionary(
                x => x.GetProperty("udi").GetString() ?? string.Empty,
                x => x);

        var html = new StringBuilder();

        foreach (var layoutItem in layoutItems.EnumerateArray())
        {
            var contentUdi = layoutItem.GetProperty("contentUdi").GetString();

            if (string.IsNullOrWhiteSpace(contentUdi) ||
                !blocksByUdi.TryGetValue(contentUdi, out var block))
            {
                continue;
            }

            html.AppendLine(RenderBlockHtml(block));
        }

        return html.ToString();
    }

    private static string RenderBlockHtml(JsonElement block)
    {
        var contentTypeKey = block.GetProperty("contentTypeKey").GetGuid();

        var text = GetJsonString(block, "text");
        var level = GetJsonInt(block, "level") ?? 2;
        var language = GetJsonString(block, "language");
        var fileName = GetJsonString(block, "fileName");
        var code = GetJsonString(block, "code");
        var diagram = GetJsonString(block, "diagram");
        var kind = GetJsonString(block, "kind");

        if (!string.IsNullOrWhiteSpace(text) && block.TryGetProperty("level", out _))
        {
            var safeLevel = Math.Clamp(level, 2, 4);

            return $"<h{safeLevel}>{HtmlEncoder.Default.Encode(text)}</h{safeLevel}>";
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return $"""
                <figure class="code-block">
                    <figcaption>{HtmlEncoder.Default.Encode(fileName ?? language ?? "code")}</figcaption>
                    <pre><code class="language-{HtmlEncoder.Default.Encode(language ?? "text")}">{HtmlEncoder.Default.Encode(code)}</code></pre>
                </figure>
                """;
        }

        if (!string.IsNullOrWhiteSpace(diagram) &&
            diagram.TrimStart().StartsWith("@startuml", StringComparison.OrdinalIgnoreCase))
        {
            var encodedDiagram = EncodePlantUml(diagram);

            return $"""
                <figure class="diagram-block plantuml-block">
                    <figcaption>PlantUML diagram</figcaption>
                    <img src="https://www.plantuml.com/plantuml/svg/{encodedDiagram}" alt="PlantUML diagram" />
                    <details>
                        <summary>PlantUML source</summary>
                        <pre><code class="language-plantuml">{HtmlEncoder.Default.Encode(diagram)}</code></pre>
                    </details>
                </figure>
                """;
        }

        if (!string.IsNullOrWhiteSpace(diagram))
        {
            return $"""
                <figure class="diagram-block mermaid-block">
                    <figcaption>Mermaid diagram</figcaption>
                    <pre class="mermaid">{HtmlEncoder.Default.Encode(diagram)}</pre>
                </figure>
                """;
        }

        if (!string.IsNullOrWhiteSpace(kind))
        {
            return $"""
                <aside class="callout callout-{HtmlEncoder.Default.Encode(kind.ToLowerInvariant())}">
                    <strong>{HtmlEncoder.Default.Encode(kind)}</strong>
                    <p>{HtmlEncoder.Default.Encode(text ?? string.Empty)}</p>
                </aside>
                """;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            return $"<p>{HtmlEncoder.Default.Encode(text)}</p>";
        }

        return string.Empty;
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.GetString()
            : null;
    }

    private static int? GetJsonInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static string EncodePlantUml(string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);

        using var output = new MemoryStream();

        using (var deflate = new DeflateStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            deflate.Write(bytes, 0, bytes.Length);
        }

        return EncodePlantUmlBytes(output.ToArray());
    }

    private static string EncodePlantUmlBytes(byte[] data)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        var result = new StringBuilder();

        for (var i = 0; i < data.Length; i += 3)
        {
            var b1 = data[i];
            var b2 = i + 1 < data.Length ? data[i + 1] : 0;
            var b3 = i + 2 < data.Length ? data[i + 2] : 0;

            result.Append(alphabet[b1 >> 2]);
            result.Append(alphabet[((b1 & 0x3) << 4) | (b2 >> 4)]);
            result.Append(alphabet[((b2 & 0xF) << 2) | (b3 >> 6)]);
            result.Append(alphabet[b3 & 0x3F]);
        }

        return result.ToString();
    }

    private static CmsCategoryDto ResolveCategory(
        IContent content,
        IReadOnlyDictionary<Guid, CmsCategoryDto> categories)
    {
        var rawCategory = GetString(content, BlogContentAliases.Category);

        if (!string.IsNullOrWhiteSpace(rawCategory))
        {
            var guidText = rawCategory
                .Replace("umb://document/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

            if (Guid.TryParse(guidText, out var categoryKey) &&
                categories.TryGetValue(categoryKey, out var category))
            {
                return category;
            }
        }

        return new CmsCategoryDto("uncategorized", "Uncategorized");
    }

    private static string? GetString(IContent content, string alias)
    {
        return content.GetValue<string>(alias);
    }

    private static DateTimeOffset? GetDateTimeOffset(IContent content, string alias)
    {
        var value = content.GetValue<DateTime?>(alias);

        return value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            : null;
    }

    private static string CreateSlug(string value)
    {
        return value.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private sealed record CmsCategoryDto(string Slug, string Name);

    public sealed record CmsPostDetailsDto(
        string Slug,
        string Title,
        string Summary,
        string Category,
        string CategorySlug,
        string Level,
        string Focus,
        IReadOnlyCollection<string> Tags,
        DateTimeOffset? PublishedDate,
        string BodyHtml);
}

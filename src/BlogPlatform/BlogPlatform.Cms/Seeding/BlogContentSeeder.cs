using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeeder
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILogger<BlogContentSeeder> _logger;

    public BlogContentSeeder(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        ILogger<BlogContentSeeder> logger)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedDocumentTypesAsync();
        SeedArticles();
    }

    private async Task SeedDocumentTypesAsync()
    {
        await CreateElementTypeAsync(
            BlogContentAliases.TextBlock,
            "Text Block",
            "icon-document",
            new[]
            {
                Property("text", "Text", "Textarea")
            });

        await CreateElementTypeAsync(
            BlogContentAliases.HeadingBlock,
            "Heading Block",
            "icon-heading-1",
            new[]
            {
                Property("level", "Level", "Numeric"),
                Property("text", "Text", "Textstring")
            });

        await CreateElementTypeAsync(
            BlogContentAliases.CodeSnippetBlock,
            "Code Snippet Block",
            "icon-code",
            new[]
            {
                Property("language", "Language", "Textstring"),
                Property("fileName", "File name", "Textstring"),
                Property("code", "Code", "Textarea")
            });

        await CreateElementTypeAsync(
            BlogContentAliases.MermaidDiagramBlock,
            "Mermaid Diagram Block",
            "icon-sitemap",
            new[]
            {
                Property("diagram", "Diagram", "Textarea")
            });

        await CreateElementTypeAsync(
            BlogContentAliases.PlantUmlDiagramBlock,
            "PlantUML Diagram Block",
            "icon-umb-deploy",
            new[]
            {
                Property("diagram", "Diagram", "Textarea")
            });

        await CreateElementTypeAsync(
            BlogContentAliases.CalloutBlock,
            "Callout Block",
            "icon-alert",
            new[]
            {
                Property("kind", "Kind", "Textstring"),
                Property("text", "Text", "Textarea")
            });

        await CreateBlogArticleTypeAsync();
    }

    private async Task CreateBlogArticleTypeAsync()
    {
        var existing = _contentTypeService.Get(BlogContentAliases.BlogArticle);

        if (existing is not null && !existing.PropertyTypeExists(BlogContentAliases.Title))
        {
            _logger.LogWarning("Broken Blog Article document type detected. Deleting and recreating it.");

            await _contentTypeService.DeleteAsync(existing.Key, Constants.Security.SuperUserKey);

            existing = null;
        }

        if (existing is not null)
        {
            _logger.LogInformation("Blog Article document type already exists and looks valid.");
            return;
        }

        var contentType = new ContentType(_shortStringHelper, -1)
        {
            Alias = BlogContentAliases.BlogArticle,
            Name = "Blog Article",
            Icon = "icon-article",
            Description = "Article managed in Umbraco and rendered by BlogPlatform.App.",
            Variations = ContentVariation.Nothing,
            AllowedAsRoot = true
        };

        await AddPropertyAsync(contentType, BlogContentAliases.Title, "Title", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Slug, "Slug", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Summary, "Summary", "Textarea", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Category, "Category", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.CategorySlug, "Category Slug", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Level, "Level", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Focus, "Focus", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Tags, "Tags", "Tags", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.PublishedDate, "Published Date", "Date Picker", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.BodyJson, "Body JSON", "Textarea", "Body");

        await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        _logger.LogInformation("Created document type: Blog Article.");
    }

    private async Task CreateElementTypeAsync(
        string alias,
        string name,
        string icon,
        IEnumerable<SeedProperty> properties)
    {
        var existing = _contentTypeService.Get(alias);

        var elementType = existing as ContentType;

        if (elementType is null)
        {
            elementType = new ContentType(_shortStringHelper, -1)
            {
                Alias = alias,
                Name = name,
                Icon = icon,
                IsElement = true,
                Variations = ContentVariation.Nothing,
                AllowedAsRoot = false
            };
        }

        foreach (var property in properties)
        {
            await AddPropertyAsync(
                elementType,
                property.Alias,
                property.Name,
                property.DataTypeName,
                "Content");
        }

        if (existing is null)
        {
            await _contentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Created element type: {Name}.", name);
        }
        else
        {
            await _contentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Updated element type: {Name}.", name);
        }
    }

    private async Task AddPropertyAsync(
        ContentType contentType,
        string alias,
        string name,
        string dataTypeName,
        string groupName)
    {
        if (contentType.PropertyTypeExists(alias))
        {
            return;
        }

        var dataType = (await _dataTypeService.GetAllAsync())
            .FirstOrDefault(x =>
                string.Equals(x.Name, dataTypeName, StringComparison.OrdinalIgnoreCase));

        if (dataType is null)
        {
            throw new InvalidOperationException($"Could not find Umbraco datatype: {dataTypeName}");
        }

        var group = contentType.PropertyGroups
            .FirstOrDefault(x =>
                string.Equals(x.Name, groupName, StringComparison.OrdinalIgnoreCase));

        if (group is null)
        {
            group = new PropertyGroup(new PropertyTypeCollection(true))
            {
                Name = groupName,
                Alias = groupName.ToLowerInvariant(),
                Type = PropertyGroupType.Tab,
                SortOrder = contentType.PropertyGroups.Count
            };

            contentType.PropertyGroups.Add(group);
        }

        if (group.PropertyTypes is null)
        {
            group.PropertyTypes = new PropertyTypeCollection(true);
        }

        group.PropertyTypes.Add(
            new PropertyType(_shortStringHelper, dataType, alias)
            {
                Name = name,
                Description = string.Empty,
                SortOrder = group.PropertyTypes.Count
            });
    }

    private void SeedArticles()
    {
        CreateArticle(
            name: "How I Structure Configuration in ASP.NET Core",
            slug: "aspnet-core-configuration-cloud-ready",
            category: "Backend (.NET)",
            categorySlug: "backend-dotnet",
            level: "Intermediate",
            focus: "Practical",
            summary: "A practical approach to appsettings, environments, Options pattern, and cloud-ready configuration.",
            tags: new[] { ".NET", "ASP.NET Core", "Configuration", "Azure" },
            bodyJson:
            """
            [
              {
                "type": "heading",
                "level": 2,
                "text": "Why configuration matters"
              },
              {
                "type": "text",
                "text": "Configuration decides how your application behaves across local development, testing, staging and production."
              },
              {
                "type": "code",
                "language": "csharp",
                "fileName": "Program.cs",
                "code": "builder.Services.Configure<MyOptions>(builder.Configuration.GetSection(\"MyOptions\"));"
              },
              {
                "type": "mermaid",
                "diagram": "flowchart LR\n    A[appsettings.json] --> B[Options]\n    B --> C[Services]\n    C --> D[Controllers]"
              },
              {
                "type": "callout",
                "kind": "tip",
                "text": "Keep configuration boring. Boring configuration is easy to deploy and easy to debug."
              }
            ]
            """);

        CreateArticle(
            name: "Using PlantUML to Document Software Architecture",
            slug: "plantuml-for-software-architecture",
            category: "Architecture",
            categorySlug: "architecture",
            level: "Beginner",
            focus: "Documentation",
            summary: "Simple diagrams that explain system context, components, deployment, and data flow.",
            tags: new[] { "PlantUML", "C4", "Diagrams", "Docs" },
            bodyJson:
            """
            [
              {
                "type": "heading",
                "level": 2,
                "text": "Why diagrams help"
              },
              {
                "type": "text",
                "text": "Architecture diagrams help explain boundaries, responsibilities and communication between systems."
              },
              {
                "type": "plantuml",
                "diagram": "@startuml\nactor Reader\nrectangle BlogPlatform.App\nrectangle BlogPlatform.Cms\nReader --> BlogPlatform.App\nBlogPlatform.App --> BlogPlatform.Cms : Delivery API\n@enduml"
              }
            ]
            """);
    }

    private void CreateArticle(
        string name,
        string slug,
        string category,
        string categorySlug,
        string level,
        string focus,
        string summary,
        string[] tags,
        string bodyJson)
    {
        var alreadyExists = _contentService
            .GetRootContent()
            .Any(x =>
                x.ContentType.Alias == BlogContentAliases.BlogArticle &&
                string.Equals(
                    x.GetValue<string>(BlogContentAliases.Slug),
                    slug,
                    StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        var article = _contentService.Create(name, -1, BlogContentAliases.BlogArticle);

        article.SetValue(BlogContentAliases.Title, name);
        article.SetValue(BlogContentAliases.Slug, slug);
        article.SetValue(BlogContentAliases.Summary, summary);
        article.SetValue(BlogContentAliases.Category, category);
        article.SetValue(BlogContentAliases.CategorySlug, categorySlug);
        article.SetValue(BlogContentAliases.Level, level);
        article.SetValue(BlogContentAliases.Focus, focus);
        article.SetValue(BlogContentAliases.Tags, string.Join(", ", tags));
        article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        article.SetValue(BlogContentAliases.BodyJson, bodyJson);

        _contentService.Save(article);
        _contentService.Publish(article, new[] { "*" });

        _logger.LogInformation("Created test article: {ArticleName}.", name);
    }

    private static SeedProperty Property(string alias, string name, string dataTypeName)
    {
        return new SeedProperty(alias, name, dataTypeName);
    }

    private sealed record SeedProperty(string Alias, string Name, string DataTypeName);
}

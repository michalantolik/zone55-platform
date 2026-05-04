using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeeder
{
    private const string ObsoleteBodyJsonAlias = "bodyJson";
    private const string BlockListEditorAlias = "Umbraco.BlockList";
    private const string BlockListLayoutAlias = "Umbraco.BlockList";

    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILogger<BlogContentSeeder> _logger;

    public BlogContentSeeder(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IShortStringHelper shortStringHelper,
        ILogger<BlogContentSeeder> logger)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _propertyEditors = propertyEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _shortStringHelper = shortStringHelper;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedDocumentTypesAsync();
        await SeedDataTypesAsync();

        SeedCategories();
        SeedArticles();
    }

    private async Task SeedDocumentTypesAsync()
    {
        await CreateElementTypeAsync(
            BlogContentAliases.TextBlock,
            "Text Block",
            "icon-document",
            new[] { Property("text", "Text", "Textarea") });

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
            new[] { Property("diagram", "Diagram", "Textarea") });

        await CreateElementTypeAsync(
            BlogContentAliases.PlantUmlDiagramBlock,
            "PlantUML Diagram Block",
            "icon-umb-deploy",
            new[] { Property("diagram", "Diagram", "Textarea") });

        await CreateElementTypeAsync(
            BlogContentAliases.CalloutBlock,
            "Callout Block",
            "icon-alert",
            new[]
            {
                Property("kind", "Kind", "Textstring"),
                Property("text", "Text", "Textarea")
            });

        await CreateBlogCategoryTypeAsync();
    }

    private async Task SeedDataTypesAsync()
    {
        await CreateBlogArticleBodyBlocksDataTypeAsync();
        await CreateBlogArticleTypeAsync();
    }

    private async Task CreateBlogCategoryTypeAsync()
    {
        var existing = _contentTypeService.Get(BlogContentAliases.BlogCategory);

        var contentType = existing as ContentType;

        if (contentType is null)
        {
            contentType = new ContentType(_shortStringHelper, -1)
            {
                Alias = BlogContentAliases.BlogCategory,
                Name = "Blog Category",
                Icon = "icon-folder",
                Description = "Category used by blog articles.",
                Variations = ContentVariation.Nothing,
                AllowedAsRoot = true
            };
        }

        await AddPropertyAsync(contentType, BlogContentAliases.Title, "Title", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Slug, "Slug", "Textstring", "Content");

        if (existing is null)
        {
            await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Created document type: Blog Category.");
        }
        else
        {
            await _contentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Updated document type: Blog Category.");
        }
    }

    private async Task CreateBlogArticleTypeAsync()
    {
        var existing = _contentTypeService.Get(BlogContentAliases.BlogArticle);

        var contentType = existing as ContentType;

        if (contentType is null)
        {
            contentType = new ContentType(_shortStringHelper, -1)
            {
                Alias = BlogContentAliases.BlogArticle,
                Name = "Blog Article",
                Icon = "icon-article",
                Description = "Article managed in Umbraco and rendered by BlogPlatform.App.",
                Variations = ContentVariation.Nothing,
                AllowedAsRoot = true
            };
        }

        await RemovePropertyIfExistsAsync(contentType, ObsoleteBodyJsonAlias);

        await AddPropertyAsync(contentType, BlogContentAliases.Title, "Title", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Slug, "Slug", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Summary, "Summary", "Textarea", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Category, "Category", "Content Picker", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Level, "Level", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Focus, "Focus", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Tags, "Tags", "Tags", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.PublishedDate, "Published Date", "Date Picker", "Content");

        await AddPropertyAsync(
            contentType,
            BlogContentAliases.BodyBlocks,
            "Body Blocks",
            BlogContentAliases.BlogArticleBodyBlocksDataTypeName,
            "Body");

        if (existing is null)
        {
            await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Created document type: Blog Article.");
        }
        else
        {
            await _contentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
            _logger.LogInformation("Updated document type: Blog Article.");
        }
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

    private async Task CreateBlogArticleBodyBlocksDataTypeAsync()
    {
        var existing = (await _dataTypeService.GetAllAsync())
            .FirstOrDefault(x =>
                string.Equals(
                    x.Name,
                    BlogContentAliases.BlogArticleBodyBlocksDataTypeName,
                    StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            _logger.LogInformation("Block List data type already exists: {Name}.", existing.Name);
            return;
        }

        var blockEditor = _propertyEditors
            .FirstOrDefault(x => x.Alias == BlockListEditorAlias);

        if (blockEditor is null)
        {
            throw new InvalidOperationException($"Could not find property editor: {BlockListEditorAlias}");
        }

        var allowedBlocks = CreateAllowedBlockConfiguration();

        var configuration = new Dictionary<string, object>
        {
            ["blocks"] = allowedBlocks,
            ["validationLimit"] = new Dictionary<string, object>
            {
                ["min"] = null!,
                ["max"] = null!
            },
            ["useSingleBlockMode"] = false
        };

        var dataType = new DataType(
            blockEditor,
            _configurationEditorJsonSerializer,
            -1)
        {
            Name = BlogContentAliases.BlogArticleBodyBlocksDataTypeName,
            DatabaseType = ValueStorageType.Ntext,
            ConfigurationData = configuration,
            EditorUiAlias = "Umb.PropertyEditorUi.BlockList"
        };

        await _dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        _logger.LogInformation(
            "Created Block List data type: {DataTypeName}.",
            BlogContentAliases.BlogArticleBodyBlocksDataTypeName);
    }

    private List<Dictionary<string, object>> CreateAllowedBlockConfiguration()
    {
        return new List<Dictionary<string, object>>
        {
            CreateAllowedBlock(BlogContentAliases.TextBlock, "{umbValue: text}"),
            CreateAllowedBlock(BlogContentAliases.HeadingBlock, "{umbValue: text}"),
            CreateAllowedBlock(BlogContentAliases.CodeSnippetBlock, "{umbValue: language} - {umbValue: fileName}"),
            CreateAllowedBlock(BlogContentAliases.MermaidDiagramBlock, "Mermaid diagram"),
            CreateAllowedBlock(BlogContentAliases.PlantUmlDiagramBlock, "PlantUML diagram"),
            CreateAllowedBlock(BlogContentAliases.CalloutBlock, "{umbValue: kind}")
        };
    }

    private Dictionary<string, object> CreateAllowedBlock(
        string elementTypeAlias,
        string label)
    {
        var elementType = _contentTypeService.Get(elementTypeAlias);

        if (elementType is null)
        {
            throw new InvalidOperationException($"Could not find element type: {elementTypeAlias}");
        }

        return new Dictionary<string, object>
        {
            ["contentElementTypeKey"] = elementType.Key,
            ["settingsElementTypeKey"] = null!,
            ["label"] = label
        };
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

    private async Task RemovePropertyIfExistsAsync(
        ContentType contentType,
        string alias)
    {
        if (!contentType.PropertyTypeExists(alias))
        {
            return;
        }

        contentType.RemovePropertyType(alias);

        await _contentTypeService.UpdateAsync(
            contentType,
            Constants.Security.SuperUserKey);

        _logger.LogInformation("Removed obsolete property: {Alias}.", alias);
    }

    private void SeedCategories()
    {
        CreateCategory("Backend (.NET)", "backend-dotnet");
        CreateCategory("Architecture", "architecture");
    }

    private void CreateCategory(string name, string slug)
    {
        var alreadyExists = _contentService
            .GetRootContent()
            .Any(x =>
                x.ContentType.Alias == BlogContentAliases.BlogCategory &&
                string.Equals(
                    x.GetValue<string>(BlogContentAliases.Slug),
                    slug,
                    StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        var category = _contentService.Create(name, -1, BlogContentAliases.BlogCategory);

        category.SetValue(BlogContentAliases.Title, name);
        category.SetValue(BlogContentAliases.Slug, slug);

        _contentService.Save(category);
        _contentService.Publish(category, new[] { "*" });

        _logger.LogInformation("Created blog category: {CategoryName}.", name);
    }

    private void SeedArticles()
    {
        CreateOrUpdateArticle(
            name: "How I Structure Configuration in ASP.NET Core",
            slug: "aspnet-core-configuration-cloud-ready",
            categorySlug: "backend-dotnet",
            level: "Intermediate",
            focus: "Practical",
            summary: "A practical approach to appsettings, environments, Options pattern, and cloud-ready configuration.",
            tags: new[] { ".NET", "ASP.NET Core", "Configuration", "Azure" },
            bodyBlocks: CreateConfigurationArticleBlocks());

        CreateOrUpdateArticle(
            name: "Using PlantUML to Document Software Architecture",
            slug: "plantuml-for-software-architecture",
            categorySlug: "architecture",
            level: "Beginner",
            focus: "Documentation",
            summary: "Simple diagrams that explain system context, components, deployment, and data flow.",
            tags: new[] { "PlantUML", "C4", "Diagrams", "Docs" },
            bodyBlocks: CreateArchitectureArticleBlocks());
    }

    private List<SeedBlock> CreateConfigurationArticleBlocks()
    {
        return new List<SeedBlock>
        {
            HeadingBlock(2, "Why configuration matters"),

            TextBlock(
                "Configuration decides how your application behaves across local development, testing, staging and production. A good configuration model keeps secrets out of source control and makes deployments predictable."),

            CodeSnippetBlock(
                language: "csharp",
                fileName: "Program.cs",
                code:
                """
                builder.Services.Configure<MyOptions>(
                    builder.Configuration.GetSection("MyOptions"));
                """),

            MermaidDiagramBlock(
                """
                flowchart LR
                    A[appsettings.json] --> B[Options pattern]
                    B --> C[Application services]
                    C --> D[Controllers / UI]
                """),

            CalloutBlock(
                kind: "tip",
                text: "Keep configuration boring. Boring configuration is easier to deploy, debug and automate."),

            TextBlock(
                "For cloud-hosted applications, the cleanest approach is to treat appsettings.json as defaults and environment variables or managed configuration services as runtime overrides.")
        };
    }

    private List<SeedBlock> CreateArchitectureArticleBlocks()
    {
        return new List<SeedBlock>
        {
            HeadingBlock(2, "Why diagrams help"),

            TextBlock(
                "Architecture diagrams help explain boundaries, responsibilities and communication between systems. They are especially useful when the system has multiple apps, APIs and infrastructure services."),

            PlantUmlDiagramBlock(
                """
                @startuml
                actor Reader
                rectangle "BlogPlatform.App" as App
                rectangle "BlogPlatform.Cms" as Cms
                rectangle "Umbraco Delivery API" as Api

                Reader --> App
                App --> Api
                Api --> Cms
                @enduml
                """),

            HeadingBlock(2, "A maintainable documentation flow"),

            MermaidDiagramBlock(
                """
                flowchart TD
                    A[Write architecture note] --> B[Add PlantUML diagram]
                    B --> C[Review with code changes]
                    C --> D[Publish article]
                """),

            CodeSnippetBlock(
                language: "plantuml",
                fileName: "context.puml",
                code:
                """
                @startuml
                actor Reader
                rectangle BlogPlatform
                Reader --> BlogPlatform
                @enduml
                """),

            CalloutBlock(
                kind: "note",
                text: "Store diagrams as text, not screenshots. Text-based diagrams are versionable, searchable and easier to maintain.")
        };
    }

    private void CreateOrUpdateArticle(
        string name,
        string slug,
        string categorySlug,
        string level,
        string focus,
        string summary,
        string[] tags,
        List<SeedBlock> bodyBlocks)
    {
        var article = _contentService
            .GetRootContent()
            .FirstOrDefault(x =>
                x.ContentType.Alias == BlogContentAliases.BlogArticle &&
                string.Equals(
                    x.GetValue<string>(BlogContentAliases.Slug),
                    slug,
                    StringComparison.OrdinalIgnoreCase));

        var isNew = article is null;

        article ??= _contentService.Create(name, -1, BlogContentAliases.BlogArticle);

        var category = FindCategoryBySlug(categorySlug);

        if (category is null)
        {
            throw new InvalidOperationException($"Category not found: {categorySlug}");
        }

        article.Name = name;
        article.SetValue(BlogContentAliases.Title, name);
        article.SetValue(BlogContentAliases.Slug, slug);
        article.SetValue(BlogContentAliases.Summary, summary);

        article.SetValue(
            BlogContentAliases.Category,
            Udi.Create(Constants.UdiEntityType.Document, category.Key).ToString());

        article.SetValue(BlogContentAliases.Level, level);
        article.SetValue(BlogContentAliases.Focus, focus);
        article.SetValue(BlogContentAliases.Tags, string.Join(", ", tags));
        article.SetValue(BlogContentAliases.PublishedDate, DateTime.UtcNow);

        article.SetValue(
            BlogContentAliases.BodyBlocks,
            CreateBlockListValueJson(bodyBlocks));

        _contentService.Save(article);
        _contentService.Publish(article, new[] { "*" });

        _logger.LogInformation(
            "{Action} test article: {ArticleName}.",
            isNew ? "Created" : "Updated",
            name);
    }

    private string CreateBlockListValueJson(IReadOnlyCollection<SeedBlock> blocks)
    {
        var layoutItems = new List<Dictionary<string, object?>>();
        var contentData = new List<Dictionary<string, object?>>();

        foreach (var block in blocks)
        {
            var elementType = _contentTypeService.Get(block.ElementTypeAlias);

            if (elementType is null)
            {
                throw new InvalidOperationException($"Could not find element type: {block.ElementTypeAlias}");
            }

            var elementUdi = Udi.Create("element", Guid.NewGuid()).ToString();

            layoutItems.Add(new Dictionary<string, object?>
            {
                ["contentUdi"] = elementUdi,
                ["settingsUdi"] = null
            });

            var content = new Dictionary<string, object?>
            {
                ["contentTypeKey"] = elementType.Key,
                ["udi"] = elementUdi
            };

            foreach (var property in block.Properties)
            {
                content[property.Key] = property.Value;
            }

            contentData.Add(content);
        }

        var blockListValue = new Dictionary<string, object?>
        {
            ["layout"] = new Dictionary<string, object?>
            {
                [BlockListLayoutAlias] = layoutItems
            },
            ["contentData"] = contentData,
            ["settingsData"] = Array.Empty<object>()
        };

        return JsonSerializer.Serialize(blockListValue);
    }

    private IContent? FindCategoryBySlug(string slug)
    {
        return _contentService
            .GetRootContent()
            .FirstOrDefault(x =>
                x.ContentType.Alias == BlogContentAliases.BlogCategory &&
                string.Equals(
                    x.GetValue<string>(BlogContentAliases.Slug),
                    slug,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static SeedBlock TextBlock(string text)
    {
        return new SeedBlock(
            BlogContentAliases.TextBlock,
            new Dictionary<string, object?>
            {
                ["text"] = text
            });
    }

    private static SeedBlock HeadingBlock(int level, string text)
    {
        return new SeedBlock(
            BlogContentAliases.HeadingBlock,
            new Dictionary<string, object?>
            {
                ["level"] = level,
                ["text"] = text
            });
    }

    private static SeedBlock CodeSnippetBlock(
        string language,
        string fileName,
        string code)
    {
        return new SeedBlock(
            BlogContentAliases.CodeSnippetBlock,
            new Dictionary<string, object?>
            {
                ["language"] = language,
                ["fileName"] = fileName,
                ["code"] = code
            });
    }

    private static SeedBlock MermaidDiagramBlock(string diagram)
    {
        return new SeedBlock(
            BlogContentAliases.MermaidDiagramBlock,
            new Dictionary<string, object?>
            {
                ["diagram"] = diagram
            });
    }

    private static SeedBlock PlantUmlDiagramBlock(string diagram)
    {
        return new SeedBlock(
            BlogContentAliases.PlantUmlDiagramBlock,
            new Dictionary<string, object?>
            {
                ["diagram"] = diagram
            });
    }

    private static SeedBlock CalloutBlock(string kind, string text)
    {
        return new SeedBlock(
            BlogContentAliases.CalloutBlock,
            new Dictionary<string, object?>
            {
                ["kind"] = kind,
                ["text"] = text
            });
    }

    private static SeedProperty Property(string alias, string name, string dataTypeName)
    {
        return new SeedProperty(alias, name, dataTypeName);
    }

    private sealed record SeedProperty(string Alias, string Name, string DataTypeName);

    private sealed record SeedBlock(
        string ElementTypeAlias,
        Dictionary<string, object?> Properties);
}

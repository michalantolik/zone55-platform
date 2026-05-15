using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Microsoft.Extensions.Options;
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
    private readonly IWebHostEnvironment _environment;
    private readonly BlogContentSeederOptions _options;

    public BlogContentSeeder(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IShortStringHelper shortStringHelper,
        IWebHostEnvironment environment,
        IOptions<BlogContentSeederOptions> options,
        ILogger<BlogContentSeeder> logger)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _propertyEditors = propertyEditors;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _shortStringHelper = shortStringHelper;
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedDocumentTypesAsync();
        await SeedDataTypesAsync();

        var seedContent = await LoadSeedContentAsync();

        SeedCategories(seedContent.Categories);
        SeedArticles(seedContent.Articles);
    }

    private async Task<BlogSeedContent> LoadSeedContentAsync()
    {
        var filePath = Path.Combine(
            _environment.ContentRootPath,
            _options.ContentFilePath);

        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Blog seed content file was not found: {filePath}",
                filePath);
        }

        await using var stream = System.IO.File.OpenRead(filePath);

        var content = await JsonSerializer.DeserializeAsync<BlogSeedContent>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return content ?? new BlogSeedContent();
    }

    private void SeedCategories(IEnumerable<BlogSeedCategory> categories)
    {
        foreach (var category in categories)
        {
            CreateCategory(category.Name, category.Slug);
        }
    }

    private void SeedArticles(IEnumerable<BlogSeedArticle> articles)
    {
        foreach (var article in articles)
        {
            CreateOrUpdateArticle(
                name: article.Name,
                slug: article.Slug,
                level: article.Level,
                focus: article.Focus,
                summary: article.Summary,
                tags: article.Tags.ToArray(),
                bodyBlocks: article.BodyBlocks.Select(CreateSeedBlock).ToList(),
                dotnetZone: article.DotnetZone,
                dotnetZoneStep: article.DotnetZoneStep);
        }
    }

    private static SeedBlock CreateSeedBlock(BlogSeedBlock block)
    {
        return block.Type switch
        {
            "text" => TextBlock(block.Text ?? string.Empty),

            "heading" => HeadingBlock(
                block.Level ?? 2,
                block.Text ?? string.Empty),

            "codeSnippet" => CodeSnippetBlock(
                block.Language ?? string.Empty,
                block.FileName ?? string.Empty,
                block.Code ?? string.Empty),

            "mermaidDiagram" => MermaidDiagramBlock(
                block.Diagram ?? string.Empty),

            "plantUmlDiagram" => PlantUmlDiagramBlock(
                block.Diagram ?? string.Empty),

            "callout" => CalloutBlock(
                block.Kind ?? "note",
                block.Text ?? string.Empty),

            _ => throw new InvalidOperationException(
                $"Unsupported seed block type: {block.Type}")
        };
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
        await RemovePropertyIfExistsAsync(contentType, "category");

        await AddPropertyAsync(contentType, BlogContentAliases.Title, "Title", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Slug, "Slug", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Summary, "Summary", "Textarea", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Level, "Level", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.Focus, "Focus", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.DotnetZone, "Dotnet Zone", "Textstring", "Content");
        await AddPropertyAsync(contentType, BlogContentAliases.DotnetZoneStep, "Dotnet Zone Step", "Textstring", "Content");
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

    private void CreateOrUpdateArticle(
        string name,
        string slug,
        string level,
        string focus,
        string summary,
        string[] tags,
        List<SeedBlock> bodyBlocks,
        string dotnetZone = "foundation",
        string dotnetZoneStep = "basic-syntax")
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

        article.Name = name;
        article.SetValue(BlogContentAliases.Title, name);
        article.SetValue(BlogContentAliases.Slug, slug);
        article.SetValue(BlogContentAliases.Summary, summary);

        article.SetValue(BlogContentAliases.Level, level);
        article.SetValue(BlogContentAliases.Focus, focus);
        article.SetValue(BlogContentAliases.DotnetZone, dotnetZone);
        article.SetValue(BlogContentAliases.DotnetZoneStep, dotnetZoneStep);
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

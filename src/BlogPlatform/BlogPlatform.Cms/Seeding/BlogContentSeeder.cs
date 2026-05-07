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

        CreateOrUpdateArticle(
            name: "ASP.NET Core Web API Fundamentals: Getting Acquainted with ASP.NET Core",
            slug: "aspnet-core-web-api-fundamentals-getting-acquainted",
            categorySlug: "backend-dotnet",
            level: "Beginner",
            focus: "Course Notes",
            summary: "A standalone study note explaining the ASP.NET Core Web API project structure, hosting model, dependency injection, middleware pipeline, environments, Swagger, CLI usage, and MVC vs Minimal API approaches.",
            tags: new[] { ".NET", "ASP.NET Core", "Web API", "Middleware", "Swagger", "Dependency Injection" },
            bodyBlocks: new List<SeedBlock>
            {
                HeadingBlock(2, "What this course module explains"),

                TextBlock(
                    "This article summarizes the first major module of an ASP.NET Core Web API fundamentals course. The goal is to understand what ASP.NET Core is, how a new Web API project is structured, how it runs, how requests flow through middleware, and how development environments affect runtime behavior."),

                TextBlock(
                    "The module uses a beginner-friendly path. It starts with the big picture, then creates a new ASP.NET Core Web API project, runs it from Visual Studio and the command line, inspects the generated files, and finally explains the request pipeline and environment-specific behavior."),

                HeadingBlock(2, "ASP.NET Core in the big picture"),

                TextBlock(
                    "ASP.NET Core is a cross-platform, high-performance, open-source framework for building modern web applications, services, APIs, cloud-enabled systems, IoT backends, and mobile backends. It can be developed on Windows, Linux, or macOS and deployed either to the cloud or on-premises."),

                TextBlock(
                    "ASP.NET Core is the web framework. .NET is the developer platform it runs on. Older versions originally ran on .NET Core, but after .NET Core evolved, Microsoft renamed the platform to .NET starting with .NET 5. Version 4 was skipped to avoid confusion with the older .NET Framework 4.x line."),

                CalloutBlock(
                    kind: "note",
                    text: ".NET 8 is a long-term support release. .NET 9 is a current release. For production learning projects, .NET 8 is often the safer baseline because it has longer support."),

                MermaidDiagramBlock(
                    """
                    flowchart TD
                        A[.NET Developer Platform] --> B[ASP.NET Core Web Framework]
                        B --> C[Web APIs]
                        B --> D[Web Apps]
                        B --> E[Cloud Services]
                        B --> F[Mobile Backends]
                        C --> G[Controllers / Minimal APIs]
                    """),

                HeadingBlock(2, "Two common ways to build APIs"),

                TextBlock(
                    "ASP.NET Core supports more than one API-building style. The course focuses on the classic ASP.NET Core MVC approach, which uses controllers and actions. This approach is full-featured, proven, and commonly used for larger APIs."),

                TextBlock(
                    "The second common style is Minimal APIs. Minimal APIs are lightweight and useful for smaller APIs, microservice-style endpoints, proxies, or scenarios where the full MVC feature set is not required."),

                MermaidDiagramBlock(
                    """
                    flowchart LR
                        A[ASP.NET Core API Options] --> B[MVC Controllers]
                        A --> C[Minimal APIs]

                        B --> B1[Full-featured]
                        B --> B2[Controllers and actions]
                        B --> B3[Good for larger APIs]

                        C --> C1[Lightweight]
                        C --> C2[Less ceremony]
                        C --> C3[Good for small services]
                    """),

                HeadingBlock(2, "Creating a new Web API project"),

                TextBlock(
                    "A new ASP.NET Core Web API project can be created from Visual Studio using the ASP.NET Core Web API template. The sample API in the course is named CityInfo.API and is intended to expose cities and their points of interest."),

                TextBlock(
                    "The generated project starts with a small structure: a project file, appsettings.json, launchSettings.json, Program.cs, and a sample WeatherForecast endpoint. The sample endpoint is useful for seeing the project run, but in the course it is removed so the real API can be built from scratch."),

                CodeSnippetBlock(
                    language: "xml",
                    fileName: "CityInfo.API.csproj",
                    code:
                    """
                    <Project Sdk="Microsoft.NET.Sdk.Web">

                      <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                        <Nullable>enable</Nullable>
                        <ImplicitUsings>enable</ImplicitUsings>
                      </PropertyGroup>

                      <ItemGroup>
                        <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
                      </ItemGroup>

                    </Project>
                    """),

                TextBlock(
                    "The Microsoft.NET.Sdk.Web SDK implicitly references the ASP.NET Core shared framework. That shared framework contains Microsoft-supported assemblies for MVC, hosting, authentication, configuration, logging, and other common web application features."),

                HeadingBlock(2, "Swagger and OpenAPI support"),

                TextBlock(
                    "The default Web API template includes Swashbuckle.AspNetCore. Swashbuckle generates an OpenAPI specification and provides a Swagger UI page that documents the API and allows basic interactive testing."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddSwaggerGen();

                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }
                    """),

                CalloutBlock(
                    kind: "tip",
                    text: "Swagger is very useful during development because it gives immediate feedback about available endpoints, HTTP methods, request shapes, and response shapes."),

                HeadingBlock(2, "launchSettings.json and local profiles"),

                TextBlock(
                    "The launchSettings.json file configures how the application starts on a local development machine. It is not meant to be deployed as production configuration. It can define multiple profiles, such as http, https, and IIS Express."),

                TextBlock(
                    "Profiles with commandName set to Project use Kestrel, the built-in cross-platform web server. IIS Express profiles use IIS Express instead. For modern ASP.NET Core development, Kestrel is often the simpler and more cross-platform choice."),

                CodeSnippetBlock(
                    language: "json",
                    fileName: "launchSettings.json",
                    code:
                    """
                    {
                      "profiles": {
                        "https": {
                          "commandName": "Project",
                          "launchBrowser": false,
                          "launchUrl": "swagger",
                          "applicationUrl": "https://localhost:7206;http://localhost:5206",
                          "environmentVariables": {
                            "ASPNETCORE_ENVIRONMENT": "Development"
                          }
                        }
                      }
                    }
                    """),

                HeadingBlock(2, "Running the API from Visual Studio and the CLI"),

                TextBlock(
                    "The application can be started from Visual Studio with F5 when debugging is needed, or Ctrl+F5 when running without the debugger. The selected launch profile controls whether the app starts with HTTP, HTTPS, or IIS Express."),

                TextBlock(
                    "The same application can also be started from the command line with the dotnet CLI. The CLI is important because build servers, automation scripts, and advanced restore or publish workflows commonly depend on it."),

                CodeSnippetBlock(
                    language: "bash",
                    fileName: "terminal",
                    code:
                    """
                    dotnet run

                    dotnet run --launch-profile https
                    """),

                CalloutBlock(
                    kind: "note",
                    text: "There is no single best way to run an ASP.NET Core application. Visual Studio, Visual Studio Code, Rider, and the dotnet CLI all use the same underlying toolchain."),

                HeadingBlock(2, "Program.cs is the application entry point"),

                TextBlock(
                    "Modern ASP.NET Core projects use top-level statements in Program.cs. That means the file does not visibly contain a Program class or Main method, but the compiler generates them behind the scenes."),

                TextBlock(
                    "Program.cs is responsible for creating the WebApplicationBuilder, registering services, building the WebApplication, configuring the request pipeline, and finally running the app."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    var builder = WebApplication.CreateBuilder(args);

                    builder.Services.AddControllers();
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddSwaggerGen();

                    var app = builder.Build();

                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                    app.UseHttpsRedirection();

                    app.UseAuthorization();

                    app.MapControllers();

                    app.Run();
                    """),

                MermaidDiagramBlock(
                    """
                    flowchart TD
                        A[Create WebApplicationBuilder] --> B[Register services]
                        B --> C[Build WebApplication]
                        C --> D[Configure middleware pipeline]
                        D --> E[Map endpoints]
                        E --> F[Run application]
                    """),

                HeadingBlock(2, "Dependency injection starts in builder.Services"),

                TextBlock(
                    "The Services collection on WebApplicationBuilder is the built-in dependency injection container. Framework services and application services are registered there so they can later be injected into controllers, services, middleware, or other components."),

                TextBlock(
                    "The AddControllers call registers the services needed for controller-based APIs. This includes controller discovery, model binding, validation support, formatters, and other MVC-related infrastructure."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    builder.Services.AddControllers();
                    """),

                CalloutBlock(
                    kind: "tip",
                    text: "Think of dependency injection as the place where the application declares what components exist and how other parts of the system can receive them."),

                HeadingBlock(2, "The request pipeline and middleware"),

                TextBlock(
                    "After the app is built, the next major concept is the request pipeline. The request pipeline defines how ASP.NET Core responds to incoming HTTP requests. It is composed of middleware components."),

                TextBlock(
                    "Middleware components are executed in the order they are added. Each middleware can do work before the next component, pass the request forward, do work after the next component, or stop the pipeline and return a response immediately."),

                MermaidDiagramBlock(
                    """
                    sequenceDiagram
                        participant Client
                        participant M1 as Middleware 1
                        participant M2 as Middleware 2
                        participant M3 as Endpoint / Controller

                        Client->>M1: HTTP request
                        M1->>M2: pass request
                        M2->>M3: pass request
                        M3-->>M2: response
                        M2-->>M1: response
                        M1-->>Client: HTTP response
                    """),

                TextBlock(
                    "Order matters. For example, if authentication or authorization middleware stops a request because the user is not allowed, later middleware and endpoint logic should not run. This is why security middleware must be placed deliberately."),

                HeadingBlock(2, "A simple inline middleware example"),

                TextBlock(
                    "The course demonstrates that the pipeline can be simplified to return the same response for every request. This helps show that ASP.NET Core only does what the configured pipeline tells it to do."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    var builder = WebApplication.CreateBuilder(args);

                    var app = builder.Build();

                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("Hello World!");
                    });

                    app.Run();
                    """),

                TextBlock(
                    "With this setup, every URL returns Hello World, including /swagger or any random path, because the pipeline does not map requests to controllers or Swagger. It simply writes one response."),

                HeadingBlock(2, "Middleware order changes behavior"),

                TextBlock(
                    "If the Hello World middleware is placed before Swagger middleware and does not pass the request forward, Swagger will never run. If Swagger is placed earlier and handles /swagger first, the documentation UI can appear for that path."),

                MermaidDiagramBlock(
                    """
                    flowchart TD
                        A[Incoming request: /swagger] --> B{Swagger middleware first?}
                        B -->|Yes| C[Show Swagger UI]
                        B -->|No| D[Hello World middleware]
                        D --> E[Return Hello World]
                    """),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                    app.Run(async context =>
                    {
                        await context.Response.WriteAsync("Hello World!");
                    });
                    """),

                CalloutBlock(
                    kind: "warning",
                    text: "Middleware order is not cosmetic. It directly changes the runtime behavior of the application."),

                HeadingBlock(2, "Working with environments"),

                TextBlock(
                    "ASP.NET Core uses the ASPNETCORE_ENVIRONMENT variable to describe the current runtime environment. Common values are Development, Staging, and Production. Custom names are also possible."),

                TextBlock(
                    "Environment is not the same thing as build configuration. Debug and Release describe how code is built. Development, Staging, and Production describe where and how the application is running."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    if (app.Environment.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }
                    """),

                TextBlock(
                    "The app.Environment property exposes information about the current hosting environment. It can provide the environment name, application name, content root path, and other runtime details."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    var environmentName = app.Environment.EnvironmentName;
                    var applicationName = app.Environment.ApplicationName;
                    var contentRootPath = app.Environment.ContentRootPath;
                    """),

                MermaidDiagramBlock(
                    """
                    flowchart LR
                        A[ASPNETCORE_ENVIRONMENT] --> B{Environment}
                        B --> C[Development]
                        B --> D[Staging]
                        B --> E[Production]

                        C --> F[Enable Swagger UI]
                        E --> G[Disable development-only middleware]
                    """),

                HeadingBlock(2, "The role of controllers"),

                TextBlock(
                    "In the MVC approach, controllers receive HTTP requests and expose actions. Later modules in the course build real controllers for cities and points of interest. The first module mainly prepares the foundation by explaining where controllers are registered and how requests eventually reach them."),

                CodeSnippetBlock(
                    language: "csharp",
                    fileName: "Program.cs",
                    code:
                    """
                    builder.Services.AddControllers();

                    var app = builder.Build();

                    app.MapControllers();
                    """),

                TextBlock(
                    "AddControllers registers the required MVC services. MapControllers adds controller endpoints to the request pipeline so incoming HTTP requests can be routed to controller actions."),

                HeadingBlock(2, "A PlantUML view of the startup model"),

                PlantUmlDiagramBlock(
                    """
                    @startuml
                    title ASP.NET Core Web API Startup Model

                    rectangle "Program.cs" as Program
                    rectangle "WebApplicationBuilder" as Builder
                    rectangle "Service Collection\nDependency Injection" as Services
                    rectangle "WebApplication" as App
                    rectangle "Middleware Pipeline" as Pipeline
                    rectangle "Controllers" as Controllers

                    Program --> Builder : CreateBuilder(args)
                    Builder --> Services : Register framework and app services
                    Services --> App : builder.Build()
                    App --> Pipeline : Configure request handling
                    Pipeline --> Controllers : Map controller endpoints

                    @enduml
                    """),

                HeadingBlock(2, "Important project files"),

                TextBlock(
                    "A beginner should understand the purpose of the generated files before adding more code. The project file defines the target framework and package references. appsettings.json stores application settings. launchSettings.json defines local launch profiles. Program.cs configures and runs the application."),

                MermaidDiagramBlock(
                    """
                    flowchart TD
                        A[ASP.NET Core Web API Project] --> B[.csproj]
                        A --> C[Program.cs]
                        A --> D[appsettings.json]
                        A --> E[launchSettings.json]
                        A --> F[Controllers]

                        B --> B1[Target framework and packages]
                        C --> C1[Services and middleware]
                        D --> D1[Application configuration]
                        E --> E1[Local development profiles]
                        F --> F1[API endpoints]
                    """),

                HeadingBlock(2, "Key lessons from the module"),

                TextBlock(
                    "The most important lesson is that ASP.NET Core applications are explicitly assembled. You register services, build the app, configure middleware, map endpoints, and run the host. Nothing magical has to happen in a hidden place."),

                TextBlock(
                    "The second important lesson is that the request pipeline is central to how ASP.NET Core works. Middleware order controls what happens to a request, whether it continues through the pipeline, and what response is eventually returned."),

                TextBlock(
                    "The third important lesson is that environment-specific behavior is normal. Development can enable tools like Swagger UI, while Production can use a stricter and more secure runtime setup."),

                CalloutBlock(
                    kind: "summary",
                    text: "At the end of this module, you should understand the ASP.NET Core big picture, project structure, Program.cs startup flow, dependency injection registration, middleware pipeline, launch profiles, CLI usage, Swagger setup, and environment-based behavior."),

                HeadingBlock(2, "Minimal mental model"),

                TextBlock(
                    "A useful mental model is this: ASP.NET Core receives an HTTP request, sends it through a configured middleware pipeline, optionally routes it to a controller action, and then sends an HTTP response back to the client."),

                MermaidDiagramBlock(
                    """
                    flowchart LR
                        A[HTTP Request] --> B[Kestrel]
                        B --> C[Middleware Pipeline]
                        C --> D[Routing]
                        D --> E[Controller Action]
                        E --> F[HTTP Response]
                    """)
            });
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

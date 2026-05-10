using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public static class LearningPathCatalog
{
    public static IReadOnlyCollection<LearningPathLevel> Build(
        IReadOnlyCollection<PostListItem> posts)
    {
        var templates = CreateTemplates();

        return templates
            .Select(level => level with
            {
                Steps = level.Steps
                    .Select(step => step with
                    {
                        Posts = MatchPosts(step, posts)
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static IReadOnlyCollection<PostListItem> MatchPosts(
        LearningPathStep step,
        IReadOnlyCollection<PostListItem> posts)
    {
        return posts
            .Where(post => IsMatch(step, post))
            .OrderByDescending(post => post.PublishedDate)
            .ThenBy(post => post.Title)
            .ToArray();
    }

    private static bool IsMatch(LearningPathStep step, PostListItem post)
    {
        var searchableText = string.Join(
            " ",
            post.Title,
            post.Summary,
            post.Category,
            post.CategorySlug,
            post.Level,
            post.Focus,
            string.Join(" ", post.Tags));

        return step.Keywords.Any(keyword =>
            searchableText.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyCollection<LearningPathLevel> CreateTemplates()
    {
        return
        [
            new LearningPathLevel(
                1,
                "Foundation",
                "Start here · Core C# and .NET fundamentals.",
                "learning-path-accent-foundation",
                [
                    CreateStep(1, 1, "Basic Syntax", "Variables, loops, control flow and first C# building blocks.", "Beginner", ["syntax", "variable", "loop", "control flow", "c# basics"]),
                    CreateStep(2, 2, "Types & Operators", "Numbers, strings, booleans, logical expressions and operators.", "Beginner", ["type", "operator", "string", "number", "boolean"]),
                    CreateStep(3, 3, "Functions & Classes", "Organizing code into methods, classes and reusable structures.", "Beginner", ["function", "method", "class", "object", "oop"]),
                    CreateStep(4, 4, "Console Apps", "First command-line applications and simple program flow.", "Beginner", ["console", "cli", "command line"])
                ]),

            new LearningPathLevel(
                2,
                "Web & App Development",
                "Build real applications, APIs and user-facing features.",
                "learning-path-accent-web",
                [
                    CreateStep(5, 1, "ASP.NET Core Ecosystem", "Understand MVC, Razor Pages, Blazor, Web API, gRPC, SignalR, hosting and the ASP.NET Core request pipeline.", "Intermediate", ["asp.net core", "asp.net", "razor pages", "mvc", "blazor", "web api", "minimal api", "grpc", "signalr", "middleware", "kestrel", "hosting", "request pipeline"]),
                    CreateStep(6, 2, "ASP.NET Core MVC", "Controllers, views, routing, models and application structure.", "Intermediate", ["asp.net", "mvc", "controller", "razor"]),
                    CreateStep(7, 3, "ASP.NET Core Web API", "Build REST APIs with controllers, endpoints, routing, DTOs, validation, status codes, OpenAPI and HTTP-based backend design.", "Intermediate", ["asp.net core web api", "web api", "rest api", "api", "controller", "endpoint", "http", "openapi", "swagger", "dto", "minimal api"]),
                    CreateStep(8, 4, "LINQ & Collections", "Modern and efficient data handling in C#.", "Intermediate", ["linq", "collection", "list", "query"]),
                    CreateStep(9, 5, "Dependency Injection", "Loose coupling, services and standard .NET application composition.", "Intermediate", ["dependency injection", "di", "service", "ioc"]),
                    CreateStep(10, 6, "Desktop Apps", "Visual applications with WPF, MAUI or desktop UI patterns.", "Optional", ["wpf", "maui", "desktop"])
                ]),

            new LearningPathLevel(
                3,
                "Architecture & Data",
                "Design maintainable, testable and data-driven systems.",
                "learning-path-accent-architecture",
                [
                    CreateStep(11, 1, "SQL Fundamentals", "Learn relational databases, SQL queries, joins, keys and basic data modeling.", "Intermediate", ["sql", "database", "relational database", "query", "join", "primary key", "foreign key"]),
                    CreateStep(12, 2, "Entity Framework Core", "Interact with databases using objects and migrations.", "Intermediate", ["entity framework", "ef core", "database", "sql", "migration"]),
                    CreateStep(13, 3, "Async Programming", "Build non-blocking applications with async and await.", "Intermediate", ["async", "await", "task", "asynchronous"]),
                    CreateStep(14, 4, "Testing", "Protect behavior with unit, integration and automated tests.", "Intermediate", ["test", "testing", "xunit", "nunit", "playwright", "selenium"]),
                    CreateStep(15, 5, "Clean Architecture", "Separate responsibilities and keep business logic maintainable.", "Advanced", ["clean architecture", "architecture", "domain", "application layer"]),
                    CreateStep(16, 6, "Design Patterns", "Reusable solutions for common software design problems.", "Advanced", ["design pattern", "patterns", "factory", "strategy", "repository"])
                ]),

            new LearningPathLevel(
                4,
                "Cloud & Advanced",
                "Cloud engineering, automation and advanced backend skills.",
                "learning-path-accent-cloud",
                [
                    CreateStep(17, 1, "Microservices", "Small independent services connected through APIs and messaging.", "Advanced", ["microservice", "distributed", "service bus", "messaging"]),
                    CreateStep(18, 2, "Azure Integration", "Deploy and scale .NET systems with Microsoft Azure.", "Advanced", ["azure", "app service", "storage", "key vault", "cloud"]),
                    CreateStep(19, 3, "CI/CD Pipelines", "Automated building, testing and deployment.", "Advanced", ["ci/cd", "pipeline", "github actions", "azure devops", "deployment"]),
                    CreateStep(20, 4, "Performance Tuning", "Benchmarking, memory, profiling and runtime efficiency.", "Advanced", ["performance", "benchmark", "memory", "profiling"]),
                    CreateStep(21, 5, "Advanced C#", "Records, pattern matching, spans and modern language features.", "Advanced", ["record", "pattern matching", "span", "advanced c#"])
                ])
        ];
    }

    private static LearningPathStep CreateStep(
        int globalOrder,
        int stepOrder,
        string title,
        string description,
        string difficulty,
        IReadOnlyCollection<string> keywords)
    {
        return new LearningPathStep(
            globalOrder,
            stepOrder,
            title,
            description,
            difficulty,
            keywords,
            []);
    }
}
using BlogPlatform.App.Models;
using BlogPlatform.Contracts.DotnetRoadmap;

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
            .OrderBy(post => GetLevelOrder(post.Level))
            .ThenByDescending(post => post.PublishedDate)
            .ThenBy(post => post.Title)
            .ToArray();
    }

    private static int GetLevelOrder(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return 50;
        }

        var normalizedLevel = level.Trim();

        if (normalizedLevel.Contains("beginner", StringComparison.OrdinalIgnoreCase)
            || normalizedLevel.Contains("basic", StringComparison.OrdinalIgnoreCase)
            || normalizedLevel.Contains("fundamental", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        if (normalizedLevel.Contains("intermediate", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (normalizedLevel.Contains("advanced", StringComparison.OrdinalIgnoreCase))
        {
            return 30;
        }

        if (normalizedLevel.Contains("expert", StringComparison.OrdinalIgnoreCase))
        {
            return 40;
        }

        return 50;
    }

    private static bool IsMatch(LearningPathStep step, PostListItem post)
    {
        if (!string.IsNullOrWhiteSpace(post.DotnetZoneStep))
        {
            return string.Equals(
                post.DotnetZoneStep,
                step.Key,
                StringComparison.OrdinalIgnoreCase);
        }

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
                DotnetRoadmapCatalog.ZoneDisplayNames[DotnetZoneKeys.Foundation],
                "Start here · Core C# and .NET fundamentals.",
                "learning-path-accent-foundation",
                [
                    CreateStep(1, 1, DotnetZoneStepKeys.CSharpDotnet, "Understand what C#, .NET, the runtime, SDK, CLI and project structure are before writing application code.", "Beginner", ["c#", ".net", "dotnet", "sdk", "runtime", "cli", "project"]),
                    CreateStep(2, 2, DotnetZoneStepKeys.BasicSyntax, "Variables, loops, control flow and first C# building blocks.", "Beginner", ["syntax", "variable", "loop", "control flow", "c# basics"]),
                    CreateStep(3, 3, DotnetZoneStepKeys.TypesOperators, "Numbers, strings, booleans, logical expressions and operators.", "Beginner", ["type", "operator", "string", "number", "boolean"]),
                    CreateStep(4, 4, DotnetZoneStepKeys.FunctionsClasses, "Organizing code into methods, classes and reusable structures.", "Beginner", ["function", "method", "class", "object", "oop"]),
                    CreateStep(5, 5, DotnetZoneStepKeys.ConsoleApps, "First command-line applications and simple program flow.", "Beginner", ["console", "cli", "command line"])
                ]),

            new LearningPathLevel(
                2,
                DotnetRoadmapCatalog.ZoneDisplayNames[DotnetZoneKeys.WebAppDevelopment],
                "Build real applications, APIs and user-facing features.",
                "learning-path-accent-web",
                [
                    CreateStep(6, 1, DotnetZoneStepKeys.AspNetCoreEcosystem, "Understand MVC, Razor Pages, Blazor, Web API, gRPC, SignalR, hosting and the ASP.NET Core request pipeline.", "Intermediate", ["asp.net core", "asp.net", "razor pages", "mvc", "blazor", "web api", "minimal api", "grpc", "signalr", "middleware", "kestrel", "hosting", "request pipeline"]),
                    CreateStep(7, 2, DotnetZoneStepKeys.AspNetCoreMvc, "Controllers, views, routing, models and application structure.", "Intermediate", ["asp.net", "mvc", "controller", "razor"]),
                    CreateStep(8, 3, DotnetZoneStepKeys.AspNetCoreWebApi, "Build REST APIs with controllers, endpoints, routing, DTOs, validation, status codes, OpenAPI and HTTP-based backend design.", "Intermediate", ["asp.net core web api", "web api", "rest api", "api", "controller", "endpoint", "http", "openapi", "swagger", "dto", "minimal api"]),
                    CreateStep(9, 4, DotnetZoneStepKeys.LinqCollections, "Modern and efficient data handling in C#.", "Intermediate", ["linq", "collection", "list", "query"]),
                    CreateStep(10, 5, DotnetZoneStepKeys.DependencyInjection, "Loose coupling, services and standard .NET application composition.", "Intermediate", ["dependency injection", "di", "service", "ioc"]),
                    CreateStep(11, 6, DotnetZoneStepKeys.DesktopApps, "Visual applications with WPF, MAUI or desktop UI patterns.", "Optional", ["wpf", "maui", "desktop"])
                ]),

            new LearningPathLevel(
                3,
                DotnetRoadmapCatalog.ZoneDisplayNames[DotnetZoneKeys.ArchitectureData],
                "Design maintainable, testable and data-driven systems.",
                "learning-path-accent-architecture",
                [
                    CreateStep(12, 1, DotnetZoneStepKeys.SqlFundamentals, "Learn relational databases, SQL queries, joins, keys and basic data modeling.", "Intermediate", ["sql", "database", "relational database", "query", "join", "primary key", "foreign key"]),
                    CreateStep(13, 2, DotnetZoneStepKeys.EntityFrameworkCore, "Interact with databases using objects and migrations.", "Intermediate", ["entity framework", "ef core", "database", "sql", "migration"]),
                    CreateStep(14, 3, DotnetZoneStepKeys.AsyncProgramming, "Build non-blocking applications with async and await.", "Intermediate", ["async", "await", "task", "asynchronous"]),
                    CreateStep(15, 4, DotnetZoneStepKeys.Testing, "Protect behavior with unit, integration and automated tests.", "Intermediate", ["test", "testing", "xunit", "nunit", "playwright", "selenium"]),
                    CreateStep(16, 5, DotnetZoneStepKeys.CleanArchitecture, "Separate responsibilities and keep business logic maintainable.", "Advanced", ["clean architecture", "architecture", "domain", "application layer"]),
                    CreateStep(17, 6, DotnetZoneStepKeys.DesignPatterns, "Reusable solutions for common software design problems.", "Advanced", ["design pattern", "patterns", "factory", "strategy", "repository"])
                ]),

            new LearningPathLevel(
                4,
                DotnetRoadmapCatalog.ZoneDisplayNames[DotnetZoneKeys.CloudAdvanced],
                "Cloud engineering, automation and advanced backend skills.",
                "learning-path-accent-cloud",
                [
                    CreateStep(18, 1, DotnetZoneStepKeys.Microservices, "Small independent services connected through APIs and messaging.", "Advanced", ["microservice", "distributed", "service bus", "messaging"]),
                    CreateStep(19, 2, DotnetZoneStepKeys.AzureIntegration, "Deploy and scale .NET systems with Microsoft Azure.", "Advanced", ["azure", "app service", "storage", "key vault", "cloud"]),
                    CreateStep(20, 3, DotnetZoneStepKeys.CicdPipelines, "Automated building, testing and deployment.", "Advanced", ["ci/cd", "pipeline", "github actions", "azure devops", "deployment"]),
                    CreateStep(21, 4, DotnetZoneStepKeys.PerformanceTuning, "Benchmarking, memory, profiling and runtime efficiency.", "Advanced", ["performance", "benchmark", "memory", "profiling"]),
                    CreateStep(22, 5, DotnetZoneStepKeys.AdvancedCSharp, "Records, pattern matching, spans and modern language features.", "Advanced", ["record", "pattern matching", "span", "advanced c#"])
                ])
        ];
    }

    private static LearningPathStep CreateStep(
        int globalOrder,
        int stepOrder,
        string key,
        string description,
        string difficulty,
        IReadOnlyCollection<string> keywords)
    {
        return new LearningPathStep(
            globalOrder,
            stepOrder,
            key,
            DotnetRoadmapCatalog.StepDisplayNames[key],
            description,
            difficulty,
            keywords,
            []);
    }
}

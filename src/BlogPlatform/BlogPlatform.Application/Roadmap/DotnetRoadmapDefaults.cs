using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Roadmap;

public static class DotnetRoadmapDefaults
{
    public static DotnetRoadmap Create()
    {
        return DotnetRoadmap.Create(
        [
            DotnetRoadmapZone.Create(
                "foundation",
                "Foundation",
                1,
                [
                    DotnetRoadmapStep.Create("csharp-dotnet", "C# & .NET", 1),
                    DotnetRoadmapStep.Create("basic-syntax", "Basic Syntax", 2),
                    DotnetRoadmapStep.Create("types-operators", "Types & Operators", 3),
                    DotnetRoadmapStep.Create("functions-classes", "Functions & Classes", 4),
                    DotnetRoadmapStep.Create("console-apps", "Console Apps", 5),
                    DotnetRoadmapStep.Create("sth", "Something", 6)
                ]),

            DotnetRoadmapZone.Create(
                "web-app-development",
                "Web & App Development",
                2,
                [
                    DotnetRoadmapStep.Create("aspnet-core-ecosystem", "ASP.NET Core Ecosystem", 1),
                    DotnetRoadmapStep.Create("aspnet-core-mvc", "ASP.NET Core MVC", 2),
                    DotnetRoadmapStep.Create("aspnet-core-web-api", "ASP.NET Core Web API", 3),
                    DotnetRoadmapStep.Create("linq-collections", "LINQ & Collections", 4),
                    DotnetRoadmapStep.Create("dependency-injection", "Dependency Injection", 5),
                    DotnetRoadmapStep.Create("desktop-apps", "Desktop Apps", 6)
                ]),

            DotnetRoadmapZone.Create(
                "architecture-data",
                "Architecture & Data",
                3,
                [
                    DotnetRoadmapStep.Create("sql-fundamentals", "SQL Fundamentals", 1),
                    DotnetRoadmapStep.Create("entity-framework-core", "Entity Framework Core", 2),
                    DotnetRoadmapStep.Create("async-programming", "Async Programming", 3),
                    DotnetRoadmapStep.Create("testing", "Testing", 4),
                    DotnetRoadmapStep.Create("clean-architecture", "Clean Architecture", 5),
                    DotnetRoadmapStep.Create("design-patterns", "Design Patterns", 6)
                ]),

            DotnetRoadmapZone.Create(
                "cloud-advanced",
                "Cloud & Advanced",
                4,
                [
                    DotnetRoadmapStep.Create("microservices", "Microservices", 1),
                    DotnetRoadmapStep.Create("azure-integration", "Azure Integration", 2),
                    DotnetRoadmapStep.Create("cicd-pipelines", "CI/CD Pipelines", 3),
                    DotnetRoadmapStep.Create("performance-tuning", "Performance Tuning", 4),
                    DotnetRoadmapStep.Create("advanced-csharp", "Advanced C#", 5)
                ])
        ]);
    }
}

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
                    DotnetRoadmapStep.Create("oop", "Object-Oriented Programming", 3),
                    DotnetRoadmapStep.Create("collections", "Collections", 4)
                ]),
            DotnetRoadmapZone.Create(
                "backend",
                "Backend",
                2,
                [
                    DotnetRoadmapStep.Create("aspnet-core", "ASP.NET Core", 1),
                    DotnetRoadmapStep.Create("rest-api", "REST API", 2),
                    DotnetRoadmapStep.Create("authentication", "Authentication", 3),
                    DotnetRoadmapStep.Create("authorization", "Authorization", 4)
                ]),
            DotnetRoadmapZone.Create(
                "database",
                "Database",
                3,
                [
                    DotnetRoadmapStep.Create("sql-server", "SQL Server", 1),
                    DotnetRoadmapStep.Create("entity-framework", "Entity Framework Core", 2),
                    DotnetRoadmapStep.Create("migrations", "Migrations", 3)
                ]),
            DotnetRoadmapZone.Create(
                "architecture",
                "Architecture",
                4,
                [
                    DotnetRoadmapStep.Create("clean-architecture", "Clean Architecture", 1),
                    DotnetRoadmapStep.Create("cqrs", "CQRS", 2),
                    DotnetRoadmapStep.Create("ddd", "DDD", 3)
                ]),
            DotnetRoadmapZone.Create(
                "cloud-devops",
                "Cloud & DevOps",
                5,
                [
                    DotnetRoadmapStep.Create("azure", "Azure", 1),
                    DotnetRoadmapStep.Create("docker", "Docker", 2),
                    DotnetRoadmapStep.Create("ci-cd", "CI/CD", 3)
                ])
        ]);
    }
}

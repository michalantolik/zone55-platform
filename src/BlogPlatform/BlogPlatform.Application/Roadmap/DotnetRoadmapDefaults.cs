using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Application.Roadmap;

public static class DotnetRoadmapDefaults
{
    public static DotnetRoadmap Create()
    {
        return new DotnetRoadmap
        {
            Zones =
            [
                new DotnetRoadmapZone
                {
                    Key = "foundation",
                    Name = "Foundation",
                    Order = 1,
                    Steps =
                    [
                        new DotnetRoadmapStep { Key = "csharp-dotnet", Name = "C# & .NET", Order = 1 },
                        new DotnetRoadmapStep { Key = "basic-syntax", Name = "Basic Syntax", Order = 2 },
                        new DotnetRoadmapStep { Key = "oop", Name = "Object-Oriented Programming", Order = 3 },
                        new DotnetRoadmapStep { Key = "collections", Name = "Collections", Order = 4 }
                    ]
                },
                new DotnetRoadmapZone
                {
                    Key = "backend",
                    Name = "Backend",
                    Order = 2,
                    Steps =
                    [
                        new DotnetRoadmapStep { Key = "aspnet-core", Name = "ASP.NET Core", Order = 1 },
                        new DotnetRoadmapStep { Key = "rest-api", Name = "REST API", Order = 2 },
                        new DotnetRoadmapStep { Key = "authentication", Name = "Authentication", Order = 3 },
                        new DotnetRoadmapStep { Key = "authorization", Name = "Authorization", Order = 4 }
                    ]
                },
                new DotnetRoadmapZone
                {
                    Key = "database",
                    Name = "Database",
                    Order = 3,
                    Steps =
                    [
                        new DotnetRoadmapStep { Key = "sql-server", Name = "SQL Server", Order = 1 },
                        new DotnetRoadmapStep { Key = "entity-framework", Name = "Entity Framework Core", Order = 2 },
                        new DotnetRoadmapStep { Key = "migrations", Name = "Migrations", Order = 3 }
                    ]
                },
                new DotnetRoadmapZone
                {
                    Key = "architecture",
                    Name = "Architecture",
                    Order = 4,
                    Steps =
                    [
                        new DotnetRoadmapStep { Key = "clean-architecture", Name = "Clean Architecture", Order = 1 },
                        new DotnetRoadmapStep { Key = "cqrs", Name = "CQRS", Order = 2 },
                        new DotnetRoadmapStep { Key = "ddd", Name = "DDD", Order = 3 }
                    ]
                },
                new DotnetRoadmapZone
                {
                    Key = "cloud-devops",
                    Name = "Cloud & DevOps",
                    Order = 5,
                    Steps =
                    [
                        new DotnetRoadmapStep { Key = "azure", Name = "Azure", Order = 1 },
                        new DotnetRoadmapStep { Key = "docker", Name = "Docker", Order = 2 },
                        new DotnetRoadmapStep { Key = "ci-cd", Name = "CI/CD", Order = 3 }
                    ]
                }
            ]
        };
    }
}

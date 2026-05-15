namespace BlogPlatform.Contracts.DotnetRoadmap;

public static class DotnetRoadmapCatalog
{
    public static readonly IReadOnlyCollection<string> AllowedZoneKeys =
    [
        DotnetZoneKeys.Foundation,
        DotnetZoneKeys.WebAppDevelopment,
        DotnetZoneKeys.ArchitectureData,
        DotnetZoneKeys.CloudAdvanced
    ];

    public static readonly IReadOnlyCollection<string> AllowedStepKeys =
    [
        DotnetZoneStepKeys.CSharpDotnet,
        DotnetZoneStepKeys.BasicSyntax,
        DotnetZoneStepKeys.TypesOperators,
        DotnetZoneStepKeys.FunctionsClasses,
        DotnetZoneStepKeys.ConsoleApps,
        DotnetZoneStepKeys.AspNetCoreEcosystem,
        DotnetZoneStepKeys.AspNetCoreMvc,
        DotnetZoneStepKeys.AspNetCoreWebApi,
        DotnetZoneStepKeys.LinqCollections,
        DotnetZoneStepKeys.DependencyInjection,
        DotnetZoneStepKeys.DesktopApps,
        DotnetZoneStepKeys.SqlFundamentals,
        DotnetZoneStepKeys.EntityFrameworkCore,
        DotnetZoneStepKeys.AsyncProgramming,
        DotnetZoneStepKeys.Testing,
        DotnetZoneStepKeys.CleanArchitecture,
        DotnetZoneStepKeys.DesignPatterns,
        DotnetZoneStepKeys.Microservices,
        DotnetZoneStepKeys.AzureIntegration,
        DotnetZoneStepKeys.CicdPipelines,
        DotnetZoneStepKeys.PerformanceTuning,
        DotnetZoneStepKeys.AdvancedCSharp
    ];

    public static readonly IReadOnlyDictionary<string, string> ZoneDisplayNames =
        new Dictionary<string, string>
        {
            [DotnetZoneKeys.Foundation] = "Foundation",
            [DotnetZoneKeys.WebAppDevelopment] = "Web & App Development",
            [DotnetZoneKeys.ArchitectureData] = "Architecture & Data",
            [DotnetZoneKeys.CloudAdvanced] = "Cloud & Advanced"
        };


    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> ZoneStepKeys =
        new Dictionary<string, IReadOnlyCollection<string>>
        {
            [DotnetZoneKeys.Foundation] =
            [
                DotnetZoneStepKeys.CSharpDotnet,
                DotnetZoneStepKeys.BasicSyntax,
                DotnetZoneStepKeys.TypesOperators,
                DotnetZoneStepKeys.FunctionsClasses,
                DotnetZoneStepKeys.ConsoleApps
            ],
            [DotnetZoneKeys.WebAppDevelopment] =
            [
                DotnetZoneStepKeys.AspNetCoreEcosystem,
                DotnetZoneStepKeys.AspNetCoreMvc,
                DotnetZoneStepKeys.AspNetCoreWebApi,
                DotnetZoneStepKeys.LinqCollections,
                DotnetZoneStepKeys.DependencyInjection,
                DotnetZoneStepKeys.DesktopApps
            ],
            [DotnetZoneKeys.ArchitectureData] =
            [
                DotnetZoneStepKeys.SqlFundamentals,
                DotnetZoneStepKeys.EntityFrameworkCore,
                DotnetZoneStepKeys.AsyncProgramming,
                DotnetZoneStepKeys.Testing,
                DotnetZoneStepKeys.CleanArchitecture,
                DotnetZoneStepKeys.DesignPatterns
            ],
            [DotnetZoneKeys.CloudAdvanced] =
            [
                DotnetZoneStepKeys.Microservices,
                DotnetZoneStepKeys.AzureIntegration,
                DotnetZoneStepKeys.CicdPipelines,
                DotnetZoneStepKeys.PerformanceTuning,
                DotnetZoneStepKeys.AdvancedCSharp
            ]
        };

    public static readonly IReadOnlyDictionary<string, string> StepDisplayNames =
        new Dictionary<string, string>
        {
            [DotnetZoneStepKeys.CSharpDotnet] = "C# & .NET",
            [DotnetZoneStepKeys.BasicSyntax] = "Basic Syntax",
            [DotnetZoneStepKeys.TypesOperators] = "Types & Operators",
            [DotnetZoneStepKeys.FunctionsClasses] = "Functions & Classes",
            [DotnetZoneStepKeys.ConsoleApps] = "Console Apps",
            [DotnetZoneStepKeys.AspNetCoreEcosystem] = "ASP.NET Core Ecosystem",
            [DotnetZoneStepKeys.AspNetCoreMvc] = "ASP.NET Core MVC",
            [DotnetZoneStepKeys.AspNetCoreWebApi] = "ASP.NET Core Web API",
            [DotnetZoneStepKeys.LinqCollections] = "LINQ & Collections",
            [DotnetZoneStepKeys.DependencyInjection] = "Dependency Injection",
            [DotnetZoneStepKeys.DesktopApps] = "Desktop Apps",
            [DotnetZoneStepKeys.SqlFundamentals] = "SQL Fundamentals",
            [DotnetZoneStepKeys.EntityFrameworkCore] = "Entity Framework Core",
            [DotnetZoneStepKeys.AsyncProgramming] = "Async Programming",
            [DotnetZoneStepKeys.Testing] = "Testing",
            [DotnetZoneStepKeys.CleanArchitecture] = "Clean Architecture",
            [DotnetZoneStepKeys.DesignPatterns] = "Design Patterns",
            [DotnetZoneStepKeys.Microservices] = "Microservices",
            [DotnetZoneStepKeys.AzureIntegration] = "Azure Integration",
            [DotnetZoneStepKeys.CicdPipelines] = "CI/CD Pipelines",
            [DotnetZoneStepKeys.PerformanceTuning] = "Performance Tuning",
            [DotnetZoneStepKeys.AdvancedCSharp] = "Advanced C#"
        };

    public static bool IsAllowedZoneKey(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        AllowedZoneKeys.Contains(value);

    public static bool IsAllowedStepKey(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        AllowedStepKeys.Contains(value);

    public static bool IsAllowedStepForZone(string? zoneKey, string? stepKey) =>
        !string.IsNullOrWhiteSpace(zoneKey) &&
        !string.IsNullOrWhiteSpace(stepKey) &&
        ZoneStepKeys.TryGetValue(zoneKey, out var stepKeys) &&
        stepKeys.Contains(stepKey);
}

public static class DotnetZoneKeys
{
    public const string Foundation = "foundation";
    public const string WebAppDevelopment = "web-app-development";
    public const string ArchitectureData = "architecture-data";
    public const string CloudAdvanced = "cloud-advanced";
}

public static class DotnetZoneStepKeys
{
    public const string CSharpDotnet = "csharp-dotnet";
    public const string BasicSyntax = "basic-syntax";
    public const string TypesOperators = "types-operators";
    public const string FunctionsClasses = "functions-classes";
    public const string ConsoleApps = "console-apps";
    public const string AspNetCoreEcosystem = "aspnet-core-ecosystem";
    public const string AspNetCoreMvc = "aspnet-core-mvc";
    public const string AspNetCoreWebApi = "aspnet-core-web-api";
    public const string LinqCollections = "linq-collections";
    public const string DependencyInjection = "dependency-injection";
    public const string DesktopApps = "desktop-apps";
    public const string SqlFundamentals = "sql-fundamentals";
    public const string EntityFrameworkCore = "entity-framework-core";
    public const string AsyncProgramming = "async-programming";
    public const string Testing = "testing";
    public const string CleanArchitecture = "clean-architecture";
    public const string DesignPatterns = "design-patterns";
    public const string Microservices = "microservices";
    public const string AzureIntegration = "azure-integration";
    public const string CicdPipelines = "cicd-pipelines";
    public const string PerformanceTuning = "performance-tuning";
    public const string AdvancedCSharp = "advanced-csharp";
}

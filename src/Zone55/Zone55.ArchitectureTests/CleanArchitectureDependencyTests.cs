using Zone55.Api.Controllers.LearnKit.Public;
using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using NetArchTest.Rules;
using System.Xml.Linq;
using ArchitectureTestResult = NetArchTest.Rules.TestResult;

namespace Zone55.ArchitectureTests;

public sealed class CleanArchitectureDependencyTests
{
    [Fact]
    public void LearnKitDomain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(typeof(Article).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "LearnKit.Application",
                "LearnKit.Infrastructure",
                "Zone55.Api",
                "Zone55.Portal",
                "Zone55.Management",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void LearnKitApplication_Should_Not_Depend_On_Infrastructure_Or_Presentation()
    {
        var result = Types.InAssembly(typeof(GetArticleBySlugQuery).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "LearnKit.Infrastructure",
                "Zone55.Api",
                "Zone55.Portal",
                "Zone55.Management",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void LearnKitInfrastructure_Should_Not_Depend_On_Presentation()
    {
        var result = Types.InAssembly(typeof(LearnKitDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Zone55.Api",
                "Zone55.Portal",
                "Zone55.Management")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Client_Applications()
    {
        var result = Types.InAssembly(typeof(ArticlesController).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zone55.Portal", "Zone55.Management")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Retired_Content_Pipeline_Should_Not_Return()
    {
        var repositoryRoot = FindRepositoryRoot();

        var retiredPaths = new[]
        {
            Path.Combine(repositoryRoot, "Dockerfile.cms"),
            Path.Combine(repositoryRoot, ".github", "workflows", "azure-seed-content.yml"),
            Path.Combine(repositoryRoot, "src", "BlogPlatform", "BlogPlatform.Application"),
            Path.Combine(repositoryRoot, "src", "BlogPlatform", "BlogPlatform.Cms"),
            Path.Combine(repositoryRoot, "src", "BlogPlatform", "BlogPlatform.Contracts"),
            Path.Combine(repositoryRoot, "src", "BlogPlatform", "BlogPlatform.Domain"),
            Path.Combine(repositoryRoot, "src", "BlogPlatform", "BlogPlatform.Infrastructure"),
            Path.Combine(repositoryRoot, "src", "Shared", "Zone55.Ui")
        };

        var existingRetiredPaths = retiredPaths
            .Where(path => File.Exists(path) || Directory.Exists(path))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        var projectFiles = Directory
            .EnumerateFiles(
                Path.Combine(repositoryRoot, "src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        var retiredProjectReferences = projectFiles
            .Select(path => new { Path = path, Xml = XDocument.Load(path) })
            .SelectMany(project => project.Xml.Descendants("ProjectReference")
                .Select(reference => new
                {
                    project.Path,
                    Include = reference.Attribute("Include")?.Value ?? string.Empty
                }))
            .Where(reference =>
                reference.Include.Contains("BlogPlatform.", StringComparison.Ordinal) ||
                reference.Include.Contains("Zone55.Ui", StringComparison.Ordinal))
            .Select(reference => $"{reference.Path}: {reference.Include}")
            .ToArray();

        var umbracoPackages = projectFiles
            .Select(path => new { Path = path, Xml = XDocument.Load(path) })
            .SelectMany(project => project.Xml.Descendants("PackageReference")
                .Select(reference => new
                {
                    project.Path,
                    Include = reference.Attribute("Include")?.Value ?? string.Empty
                }))
            .Where(reference =>
                reference.Include.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase))
            .Select(reference => $"{reference.Path}: {reference.Include}")
            .ToArray();

        Assert.True(
            existingRetiredPaths.Length == 0,
            "Retired content pipeline paths returned: " +
            string.Join(", ", existingRetiredPaths));

        Assert.True(
            retiredProjectReferences.Length == 0,
            "Projects reference retired content pipeline projects: " +
            string.Join(", ", retiredProjectReferences));

        Assert.True(
            umbracoPackages.Length == 0,
            "Projects reference retired Umbraco packages: " +
            string.Join(", ", umbracoPackages));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string BuildMessage(ArchitectureTestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : string.Join(Environment.NewLine, result.FailingTypeNames ?? []);
}

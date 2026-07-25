using Zone55.Api.Controllers.LearnKit.Public;
using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using LearnKit.Domain.Articles;
using LearnKit.Infrastructure.Persistence;
using NetArchTest.Rules;
using System.Xml.Linq;

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
                "Zone55.App",
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
                "Zone55.App",
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
                "Zone55.App",
                "Zone55.Management")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Client_Applications()
    {
        var result = Types.InAssembly(typeof(ArticlesController).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zone55.App", "Zone55.Management")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Active_Projects_Should_Not_Reference_Retired_Projects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var activeSourceRoots = new[]
        {
            Path.Combine(repositoryRoot, "src", "LearnKit"),
            Path.Combine(repositoryRoot, "src", "Zone55")
        };

        var projectFiles = activeSourceRoots
            .SelectMany(path => Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        var retiredNames = new[]
        {
            "BlogPlatform.Cms",
            "BlogPlatform.Application",
            "BlogPlatform.Contracts",
            "BlogPlatform.Domain",
            "BlogPlatform.Infrastructure"
        };

        var violations = projectFiles
            .Select(path => new { Path = path, Xml = XDocument.Load(path) })
            .SelectMany(project => project.Xml.Descendants("ProjectReference")
                .Select(reference => new
                {
                    project.Path,
                    Include = reference.Attribute("Include")?.Value ?? string.Empty
                }))
            .Where(reference => retiredNames.Any(name =>
                reference.Include.Contains(name, StringComparison.Ordinal)))
            .Select(reference => $"{reference.Path}: {reference.Include}")
            .ToArray();

        Assert.True(violations.Length == 0,
            "Active projects reference retired projects: " + string.Join(", ", violations));
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

    private static string BuildMessage(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : string.Join(Environment.NewLine, result.FailingTypeNames ?? []);
}

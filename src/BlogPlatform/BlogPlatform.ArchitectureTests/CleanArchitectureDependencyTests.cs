using BlogPlatform.Api.Controllers;
using BlogPlatform.Application.Posts;
using BlogPlatform.Cms.Controllers;
using BlogPlatform.Cms.Seeding;
using BlogPlatform.Contracts.Posts;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Cms;
using NetArchTest.Rules;
using System.Xml.Linq;

namespace BlogPlatform.ArchitectureTests;

public sealed class CleanArchitectureDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types
            .InAssembly(typeof(Post).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Application",
                "BlogPlatform.Infrastructure",
                "BlogPlatform.Api",
                "BlogPlatform.Cms",
                "BlogPlatform.App",
                "BlogPlatform.Contracts",
                "Microsoft.Extensions",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Umbraco.Cms")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types
            .InAssembly(typeof(IBlogPostQueryService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Infrastructure",
                "BlogPlatform.Api",
                "BlogPlatform.Cms",
                "BlogPlatform.App",
                "BlogPlatform.Contracts",
                "Microsoft.AspNetCore",
                "Microsoft.EntityFrameworkCore",
                "Umbraco.Cms")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Presentation_Or_Contracts()
    {
        var result = Types
            .InAssembly(typeof(UmbracoDeliveryApiBlogPostRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Api",
                "BlogPlatform.Cms",
                "BlogPlatform.App",
                "BlogPlatform.Contracts",
                "Umbraco.Cms")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Contracts_Should_Be_Independent()
    {
        var result = Types
            .InAssembly(typeof(PostListItemDto).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Application",
                "BlogPlatform.Domain",
                "BlogPlatform.Infrastructure",
                "BlogPlatform.Api",
                "BlogPlatform.Cms",
                "BlogPlatform.App")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Cms_App_Or_Domain()
    {
        var result = Types
            .InAssembly(typeof(PostsController).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Cms",
                "BlogPlatform.App",
                "BlogPlatform.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Api_Controllers_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(PostsController).Assembly)
            .That()
            .ResideInNamespace("BlogPlatform.Api.Controllers")
            .ShouldNot()
            .HaveDependencyOn("BlogPlatform.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Cms_Should_Not_Depend_On_Domain_Directly()
    {
        var result = Types
            .InAssembly(typeof(BlogContentController).Assembly)
            .ShouldNot()
            .HaveDependencyOn("BlogPlatform.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Cms_Controllers_Should_Not_Depend_On_Infrastructure_Or_Umbraco_Core()
    {
        var result = Types
            .InAssembly(typeof(BlogContentController).Assembly)
            .That()
            .ResideInNamespace("BlogPlatform.Cms.Controllers")
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Infrastructure",
                "Umbraco.Cms.Core",
                "Umbraco.Cms.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Cms_Umbraco_Dependencies_Should_Stay_In_Cms_Integration_Areas()
    {
        var result = Types
            .InAssembly(typeof(BlogContentSeeder).Assembly)
            .That()
            .DoNotResideInNamespace("BlogPlatform.Cms.Controllers")
            .Should()
            .NotBeAbstract()
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void App_Should_Not_Depend_On_Inner_Implementation_Layers()
    {
        var result = Types
            .InAssembly(typeof(BlogPlatform.App.Services.IBlogApiClient).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Domain",
                "BlogPlatform.Application",
                "BlogPlatform.Infrastructure",
                "BlogPlatform.Api",
                "BlogPlatform.Cms")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Project_References_Should_Follow_Clean_Architecture_Direction()
    {
        var root = FindSolutionRoot();

        AssertProjectReferences(root, "BlogPlatform.Domain", []);
        AssertProjectReferences(root, "BlogPlatform.Application", ["BlogPlatform.Domain"]);
        AssertProjectReferences(root, "BlogPlatform.Infrastructure", ["BlogPlatform.Application", "BlogPlatform.Domain"]);
        AssertProjectReferences(root, "BlogPlatform.Contracts", []);
        AssertProjectReferences(root, "BlogPlatform.App", ["BlogPlatform.Contracts"]);
        AssertProjectReferences(root, "BlogPlatform.Api", ["BlogPlatform.Application", "BlogPlatform.Contracts", "BlogPlatform.Infrastructure"]);
        AssertProjectReferences(root, "BlogPlatform.Cms", ["BlogPlatform.Application", "BlogPlatform.Contracts", "BlogPlatform.Infrastructure"]);
    }

    private static void AssertProjectReferences(
        DirectoryInfo solutionRoot,
        string projectName,
        IReadOnlyCollection<string> allowedReferences)
    {
        var projectFile = Directory
            .EnumerateFiles(solutionRoot.FullName, $"{projectName}.csproj", SearchOption.AllDirectories)
            .Single();

        var document = XDocument.Load(projectFile);

        var actualReferences = document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(value => value)
            .ToList();

        var expectedReferences = allowedReferences
            .OrderBy(value => value)
            .ToList();

        Assert.Equal(expectedReferences, actualReferences);
    }

    private static DirectoryInfo FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BlogPlatform.slnx")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find BlogPlatform.slnx.");
    }

    private static string BuildMessage(TestResult result)
    {
        if (result.FailingTypeNames is null || !result.FailingTypeNames.Any())
        {
            return "Architecture rule failed.";
        }

        return "Architecture rule failed for: " +
               string.Join(", ", result.FailingTypeNames);
    }
}

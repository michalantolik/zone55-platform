using BlogPlatform.Api.Controllers.LearnKit;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Controllers;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Roadmap;
using NetArchTest.Rules;
using System.Reflection;
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
            .InAssembly(typeof(IRoadmapQueryService).Assembly)
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
    public void Application_Should_Not_Reference_Infrastructure_Implementations()
    {
        var result = Types
            .InAssembly(typeof(IRoadmapQueryService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Infrastructure.Cms",
                "BlogPlatform.Infrastructure.Persistence",
                "BlogPlatform.Infrastructure.Roadmap")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Presentation_Or_Contracts()
    {
        var result = Types
            .InAssembly(typeof(SqlDotnetRoadmapStore).Assembly)
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
    public void Api_Should_Not_Depend_On_Cms_App_Or_Domain()
    {
        var result = Types
            .InAssembly(typeof(ArticlesController).Assembly)
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
            .InAssembly(typeof(ArticlesController).Assembly)
            .That()
            .ResideInNamespace("BlogPlatform.Api.Controllers")
            .ShouldNot()
            .HaveDependencyOn("BlogPlatform.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Api_Mapping_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(ArticlesController).Assembly)
            .That()
            .ResideInNamespace("BlogPlatform.Api.Mapping")
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
    public void Cms_Program_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(BlogContentController).Assembly)
            .That()
            .HaveName("Program")
            .ShouldNot()
            .HaveDependencyOn("BlogPlatform.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Cms_Public_Controller_Surface_Should_Not_Expose_Umbraco_Types()
    {
        var controllerTypes = typeof(BlogContentController)
            .Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                type.Namespace == "BlogPlatform.Cms.Controllers")
            .ToList();

        var violations = controllerTypes
            .SelectMany(GetPublicSurfaceTypes)
            .Where(type =>
                type.FullName?.StartsWith(
                    "Umbraco.Cms",
                    StringComparison.Ordinal) == true)
            .Select(type => type.FullName)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        Assert.True(
            violations.Count == 0,
            "CMS controller public surface exposes Umbraco types: " +
            string.Join(", ", violations));
    }

    [Fact]
    public void App_Should_Not_Depend_On_Inner_Implementation_Layers()
    {
        var result = Types
            .InAssembly(
                typeof(
                    BlogPlatform.App.Services.LearnKit.ILearnKitApiClient)
                .Assembly)
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
    public void App_LearnKit_Services_Should_Not_Depend_On_Api_Application_Or_Infrastructure()
    {
        var result = Types
            .InAssembly(
                typeof(
                    BlogPlatform.App.Services.LearnKit.ILearnKitApiClient)
                .Assembly)
            .That()
            .ResideInNamespace("BlogPlatform.App.Services.LearnKit")
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Api",
                "BlogPlatform.Application",
                "BlogPlatform.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Project_References_Should_Follow_Clean_Architecture_Direction()
    {
        var root = FindSolutionRoot();

        AssertProjectReferences(
            root,
            "BlogPlatform.Domain",
            []);

        AssertProjectReferences(
            root,
            "BlogPlatform.Application",
            ["BlogPlatform.Domain"]);

        AssertProjectReferences(
            root,
            "BlogPlatform.Infrastructure",
            [
                "BlogPlatform.Application",
                "BlogPlatform.Domain"
            ]);

        AssertProjectReferences(
            root,
            "BlogPlatform.Contracts",
            []);

        AssertProjectReferences(
            root,
            "BlogPlatform.App",
            []);

        AssertProjectReferences(
            root,
            "BlogPlatform.Api",
            [
                "LearnKit.Application",
                "LearnKit.Infrastructure"
            ]);

        AssertProjectReferences(
            root,
            "BlogPlatform.Cms",
            [
                "BlogPlatform.Application",
                "BlogPlatform.Infrastructure"
            ]);
    }

    [Fact]
    public void Application_Project_Should_Only_Reference_Domain()
    {
        var root = FindSolutionRoot();

        AssertProjectReferences(
            root,
            "BlogPlatform.Application",
            ["BlogPlatform.Domain"]);
    }

    [Fact]
    public void Contracts_Project_Should_Not_Reference_Other_Projects()
    {
        var root = FindSolutionRoot();

        AssertProjectReferences(
            root,
            "BlogPlatform.Contracts",
            []);
    }

    private static IEnumerable<Type> GetPublicSurfaceTypes(Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }

        foreach (var method in type.GetMethods(
                     BindingFlags.Public |
                     BindingFlags.Instance |
                     BindingFlags.DeclaredOnly))
        {
            yield return method.ReturnType;

            foreach (var parameter in method.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }

        foreach (var property in type.GetProperties(
                     BindingFlags.Public |
                     BindingFlags.Instance |
                     BindingFlags.DeclaredOnly))
        {
            yield return property.PropertyType;
        }
    }

    private static void AssertProjectReferences(
        DirectoryInfo solutionRoot,
        string projectName,
        IReadOnlyCollection<string> allowedReferences)
    {
        var projectFile = Directory
            .EnumerateFiles(
                solutionRoot.FullName,
                $"{projectName}.csproj",
                SearchOption.AllDirectories)
            .Single();

        var document = XDocument.Load(projectFile);

        var actualReferences = document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => GetProjectNameFromReference(value!))
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
            if (File.Exists(
                    Path.Combine(
                        directory.FullName,
                        "BlogPlatform.slnx")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find BlogPlatform.slnx.");
    }

    private static string BuildMessage(
        NetArchTest.Rules.TestResult result)
    {
        if (result.FailingTypeNames is null ||
            !result.FailingTypeNames.Any())
        {
            return "Architecture rule failed.";
        }

        return "Architecture rule failed for: " +
               string.Join(", ", result.FailingTypeNames);
    }

    private static string GetProjectNameFromReference(
        string projectReference)
    {
        var normalizedReference = projectReference
            .Replace('\\', '/');

        return Path.GetFileNameWithoutExtension(
            normalizedReference);
    }
}
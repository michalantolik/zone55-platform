using BlogPlatform.Api.Controllers;
using BlogPlatform.Application.Posts;
using BlogPlatform.Cms.Controllers;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Cms;
using NetArchTest.Rules;

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
                "BlogPlatform.Contracts")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Presentation_Or_Contracts()
    {
        var result = Types
            .InAssembly(typeof(IBlogPostQueryService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Infrastructure",
                "BlogPlatform.Api",
                "BlogPlatform.Cms",
                "BlogPlatform.App",
                "BlogPlatform.Contracts")
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
                "BlogPlatform.Contracts")
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
    public void Cms_Should_Not_Depend_On_Api_App_Or_Domain()
    {
        var result = Types
            .InAssembly(typeof(BlogContentController).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BlogPlatform.Api",
                "BlogPlatform.App",
                "BlogPlatform.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result));
    }

    [Fact]
    public void Controllers_Should_Not_Contain_Domain_Logic()
    {
        var result = Types
            .InAssemblies([
                typeof(PostsController).Assembly,
                typeof(BlogContentController).Assembly
            ])
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("BlogPlatform.Domain")
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

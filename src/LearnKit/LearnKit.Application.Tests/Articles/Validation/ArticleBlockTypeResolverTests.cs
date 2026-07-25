using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Application.Tests.Articles.Validation;

public sealed class ArticleBlockTypeResolverTests
{
    [Theory]
    [InlineData("Markdown", ArticleBlockType.Markdown)]
    [InlineData("heading", ArticleBlockType.Markdown)]
    [InlineData("text", ArticleBlockType.Markdown)]
    [InlineData("Code", ArticleBlockType.Code)]
    [InlineData("codeSnippet", ArticleBlockType.Code)]
    [InlineData("Diagram", ArticleBlockType.Diagram)]
    [InlineData("plantUmlDiagram", ArticleBlockType.Diagram)]
    [InlineData("mermaidDiagram", ArticleBlockType.Diagram)]
    [InlineData("Table", ArticleBlockType.Table)]
    [InlineData("Callout", ArticleBlockType.Callout)]
    [InlineData("Summary", ArticleBlockType.Summary)]
    public void TryResolve_ShouldMapCanonicalAndCompatibleNames(
        string value,
        ArticleBlockType expected)
    {
        var resolved = ArticleBlockTypeResolver.TryResolve(value, out var actual);

        Assert.True(resolved);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryResolve_ShouldRejectUnknownType()
    {
        Assert.False(ArticleBlockTypeResolver.TryResolve("video", out _));
    }
}

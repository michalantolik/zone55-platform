using BlogPlatform.App.Components.Articles.LearnKitRendering.Blocks;
using BlogPlatform.App.Components.Articles.LearnKitRendering.Provider;
using BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization;
using BlogPlatform.App.Models.LearnKit.Articles;
using System.Text.Json;
using Zone55.Management.Services;

namespace BlogPlatform.Presentation.Tests;

public sealed class LearnKitArticlePreviewMapperTests
{
    [Theory]
    [InlineData("text", "Markdown", typeof(MarkdownBlock))]
    [InlineData("heading", "Markdown", typeof(MarkdownBlock))]
    [InlineData("codeSnippet", "Code", typeof(CodeBlock))]
    [InlineData("plantUmlDiagram", "Diagram", typeof(DiagramBlock))]
    [InlineData("mermaidDiagram", "Diagram", typeof(DiagramBlock))]
    [InlineData("table", "Table", typeof(TableBlock))]
    [InlineData("callout", "Callout", typeof(CalloutBlock))]
    [InlineData("summary", "Summary", typeof(SummaryBlock))]
    public void ToArticle_ShouldNormalizeLegacyBlockAliases(
        string sourceType,
        string expectedType,
        Type expectedComponent)
    {
        var payload = new LearnKitArticlePreviewPayload
        {
            Slug = "preview",
            Title = "Preview",
            BodyContent = $$"""[{"type":"{{sourceType}}","sortOrder":1,"text":"Content","code":"code","diagram":"diagram","rows":[],"summary":"summary"}]"""
        };

        var article = LearnKitArticlePreviewMapper.ToArticle(payload);
        var block = Assert.Single(article.Blocks);

        Assert.Equal(expectedType, block.Type);
        Assert.Equal(expectedComponent, new LearnKitBlockComponentTypeProvider().GetComponentType(block));
    }

    [Fact]
    public void ToArticle_ShouldKeepCanonicalEnvelopeType_WhenContentContainsLegacyType()
    {
        var body = ArticlePreviewBlockPayloadBuilder.Build(
        [
            new ArticlePreviewBlockPayload(
                "block-1",
                "Markdown",
                "{\"type\":\"text\",\"text\":\"Legacy text\"}",
                1),
            new ArticlePreviewBlockPayload(
                "block-2",
                "Diagram",
                "{\"type\":\"plantUmlDiagram\",\"diagram\":\"@startuml\\n@enduml\",\"diagramType\":\"PlantUml\"}",
                2)
        ]);

        var article = LearnKitArticlePreviewMapper.ToArticle(new LearnKitArticlePreviewPayload
        {
            Slug = "preview",
            Title = "Preview",
            BodyContent = body
        });

        Assert.Collection(
            article.Blocks,
            markdown =>
            {
                Assert.Equal("block-1", markdown.Id);
                Assert.Equal("Markdown", markdown.Type);
                using var content = JsonDocument.Parse(markdown.ContentJson);
                Assert.Equal("text", content.RootElement.GetProperty("type").GetString());
            },
            diagram =>
            {
                Assert.Equal("block-2", diagram.Id);
                Assert.Equal("Diagram", diagram.Type);
                using var content = JsonDocument.Parse(diagram.ContentJson);
                Assert.Equal("plantUmlDiagram", content.RootElement.GetProperty("type").GetString());
            });
    }

    [Fact]
    public void Builder_ShouldRenderSafeFallback_WhenDraftJsonIsInvalid()
    {
        var body = ArticlePreviewBlockPayloadBuilder.Build(
        [
            new ArticlePreviewBlockPayload("block-1", "Code", "{", 1)
        ]);

        var article = LearnKitArticlePreviewMapper.ToArticle(new LearnKitArticlePreviewPayload
        {
            Slug = "preview",
            Title = "Preview",
            BodyContent = body
        });

        var block = Assert.Single(article.Blocks);
        Assert.Equal("Markdown", block.Type);
        Assert.Contains("invalid JSON", block.ContentJson, StringComparison.OrdinalIgnoreCase);
    }
}

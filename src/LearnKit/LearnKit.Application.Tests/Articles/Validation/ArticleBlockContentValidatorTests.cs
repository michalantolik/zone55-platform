using LearnKit.Domain.Articles;

namespace LearnKit.Application.Tests.Articles.Validation;

public sealed class ArticleBlockContentValidatorTests
{
    [Theory]
    [InlineData(ArticleBlockType.Markdown, "{\"markdown\":\"Content\"}")]
    [InlineData(ArticleBlockType.Markdown, "{\"text\":\"Legacy content\"}")]
    [InlineData(ArticleBlockType.Code, "{\"code\":\"dotnet test\",\"language\":\"bash\"}")]
    [InlineData(ArticleBlockType.Diagram, "{\"diagram\":\"@startuml\\n@enduml\",\"diagramType\":\"PlantUml\"}")]
    [InlineData(ArticleBlockType.Table, "{\"rows\":[[{\"text\":\"Name\"}]]}")]
    [InlineData(ArticleBlockType.Callout, "{\"kind\":\"info\",\"text\":\"Important\"}")]
    [InlineData(ArticleBlockType.Summary, "{\"summary\":\"Summary\"}")]
    public void Validate_ShouldAcceptContentMatchingBlockType(
        ArticleBlockType blockType,
        string contentJson)
    {
        var exception = Record.Exception(
            () => ArticleBlockContentValidator.Validate(blockType, contentJson));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(ArticleBlockType.Markdown, "{}", "markdown")]
    [InlineData(ArticleBlockType.Code, "{\"language\":\"csharp\"}", "code")]
    [InlineData(ArticleBlockType.Diagram, "{\"title\":\"Flow\"}", "diagram")]
    [InlineData(ArticleBlockType.Table, "{\"rows\":{}}", "array")]
    [InlineData(ArticleBlockType.Callout, "{\"kind\":\"warning\"}", "text")]
    [InlineData(ArticleBlockType.Summary, "{}", "summary")]
    public void Validate_ShouldRejectContentMissingRequiredContract(
        ArticleBlockType blockType,
        string contentJson,
        string expectedErrorFragment)
    {
        var exception = Assert.Throws<ArticleBlockContentValidationException>(
            () => ArticleBlockContentValidator.Validate(blockType, contentJson));

        Assert.Contains(
            exception.Errors,
            error => error.Contains(expectedErrorFragment, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldRejectMalformedJson()
    {
        var exception = Assert.Throws<ArticleBlockContentValidationException>(
            () => ArticleBlockContentValidator.Validate(
                ArticleBlockType.Markdown,
                "{\"markdown\":"));

        Assert.Contains(exception.Errors, error => error.Contains("valid JSON"));
    }

    [Fact]
    public void Update_ShouldNotChangeBlock_WhenNewContentIsInvalid()
    {
        var block = new ArticleBlock(
            ArticleBlockType.Markdown,
            1,
            "{\"markdown\":\"Original\"}");

        Assert.Throws<ArticleBlockContentValidationException>(
            () => block.Update(ArticleBlockType.Code, "{}"));

        Assert.Equal(ArticleBlockType.Markdown, block.Type);
        Assert.Equal("{\"markdown\":\"Original\"}", block.ContentJson);
    }
}

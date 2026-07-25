using Zone55.Portal.Components.Articles.Shared;

namespace Zone55.Presentation.Tests;

public sealed class DiagramRendererTests
{
    private static readonly PlantUmlThemeValues Theme = new(
        "#8FA9C8",
        "#E5F0FF",
        "#5F7898",
        "#1B2B43",
        "Arial");

    [Fact]
    public void CreatePlantUmlUrl_ShouldUseCompressedPlantUmlEncoding()
    {
        var source = """
            @startuml
            Alice -> Bob: Hello
            @enduml
            """;

        var url = DiagramRenderer.CreatePlantUmlUrl(source, Theme);
        var encoded = url[(url.LastIndexOf('/') + 1)..];

        Assert.StartsWith("https://www.plantuml.com/plantuml/svg/", url, StringComparison.Ordinal);
        Assert.DoesNotContain("~h", encoded, StringComparison.Ordinal);
        Assert.DoesNotContain('=', encoded);
        Assert.True(encoded.Length < 1500, $"Encoded PlantUML URL segment is unexpectedly long: {encoded.Length}.");
    }

    [Fact]
    public void CreatePlantUmlUrl_ShouldReturnEmptyValue_ForBlankSource()
    {
        Assert.Equal(string.Empty, DiagramRenderer.CreatePlantUmlUrl(" ", Theme));
    }
}

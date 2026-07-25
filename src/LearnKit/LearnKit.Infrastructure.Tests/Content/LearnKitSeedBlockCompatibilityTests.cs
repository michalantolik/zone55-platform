using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.DomainModel;
using System.Text.Json;

namespace LearnKit.Infrastructure.Tests.Content;

public sealed class LearnKitSeedBlockCompatibilityTests
{
    [Fact]
    public void Seed_ShouldContainOnlySupportedAndValidLearnKitBlocks()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "Content",
            "learnkit-content.seed.json");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var blocks = FindProperties(document.RootElement, "blocks")
            .Where(element => element.ValueKind == JsonValueKind.Array)
            .SelectMany(element => element.EnumerateArray())
            .ToArray();

        Assert.NotEmpty(blocks);

        foreach (var block in blocks)
        {
            var typeElement = block.GetProperty("type");
            var type = typeElement.ValueKind switch
            {
                JsonValueKind.Number when typeElement.TryGetInt32(out var numericType) &&
                                          Enum.IsDefined(typeof(ArticleBlockType), numericType) =>
                    (ArticleBlockType)numericType,
                JsonValueKind.String when ArticleBlockTypeResolver.TryResolve(typeElement.GetString(), out var resolvedType) =>
                    resolvedType,
                _ => throw new Xunit.Sdk.XunitException($"Unsupported seed block type: {typeElement.GetRawText()}")
            };

            var contentJson = block.GetProperty("content").GetRawText();
            var exception = Record.Exception(() => ArticleBlockContentValidator.Validate(type, contentJson));

            Assert.Null(exception);
        }
    }

    private static IEnumerable<JsonElement> FindProperties(
        JsonElement element,
        string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName))
                {
                    yield return property.Value;
                }

                foreach (var nested in FindProperties(property.Value, propertyName))
                {
                    yield return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in FindProperties(item, propertyName))
                {
                    yield return nested;
                }
            }
        }
    }
}

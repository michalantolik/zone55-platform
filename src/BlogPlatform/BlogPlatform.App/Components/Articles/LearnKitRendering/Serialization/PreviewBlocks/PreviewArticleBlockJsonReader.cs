using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

public static class PreviewArticleBlockJsonReader
{
    public static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.GetRawText()
            : null;
    }

    public static string? GetFirstString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetString(element, propertyName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    public static bool HasString(JsonElement element, string propertyName)
    {
        return !string.IsNullOrWhiteSpace(GetString(element, propertyName));
    }

    public static bool HasAnyString(JsonElement element, params string[] propertyNames)
    {
        return propertyNames.Any(propertyName => HasString(element, propertyName));
    }

    public static bool HasProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out _);
    }

    public static bool? GetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }

    public static int? GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.TryGetInt32(out var number)
            ? number
            : null;
    }
}

using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Helpers;

public static class LearnKitBlockJsonHelper
{
    public static JsonElement Parse(string contentJson)
    {
        using var document = JsonDocument.Parse(contentJson);

        return document.RootElement.Clone();
    }

    public static string? GetString(JsonElement json, string propertyName)
    {
        return json.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString()
            : null;
    }

    public static bool GetBoolean(JsonElement json, string propertyName, bool fallback = false)
    {
        return json.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : fallback;
    }

    public static int GetInt32(JsonElement json, string propertyName, int fallback = 0)
    {
        return json.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : fallback;
    }
}

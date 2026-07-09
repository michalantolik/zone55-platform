using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Helpers;

public static class LearnKitBlockJsonHelper
{
    public static JsonElement Parse(string contentJson)
    {
        using var document = JsonDocument.Parse(contentJson);

        return document.RootElement.Clone();
    }
}

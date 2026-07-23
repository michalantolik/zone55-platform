using System.Text.Json.Serialization;

namespace BlogPlatform.App.Models.LearnKit.Articles;

/// <summary>
/// Represents draft article data sent by the Zone55 Management live preview.
/// This transport model exists only at the Management-to-App preview boundary.
/// </summary>
public sealed class LearnKitArticlePreviewPayload
{
    public string Slug { get; init; } = "preview";

    public string Title { get; init; } = "Untitled article";

    public string? Summary { get; init; }

    [JsonPropertyName("bodyHtml")]
    public string? BodyContent { get; init; }
}

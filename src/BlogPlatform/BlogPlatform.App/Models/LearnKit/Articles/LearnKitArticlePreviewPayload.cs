using System.Text.Json.Serialization;

namespace BlogPlatform.App.Models.LearnKit.Articles;

/// <summary>
/// Represents the draft article data sent by the CMS live preview editor.
/// This transport model exists only at the CMS-to-App preview boundary.
/// </summary>
public sealed class LearnKitArticlePreviewPayload
{
    public string Slug { get; init; } = "preview";

    public string Title { get; init; } = "Untitled article";

    public string? Summary { get; init; }

    [JsonPropertyName("bodyHtml")]
    public string? BodyContent { get; init; }
}

namespace BlogPlatform.Contracts.Posts;

public sealed class ArticleBlockDto
{
    public ArticleBlockDto(ArticleBlockType type)
    {
        Type = type;
    }

    public ArticleBlockType Type { get; }

    public string? Text { get; init; }

    public int Level { get; init; } = 2;

    public string? Code { get; init; }

    public string? Language { get; init; }

    public string? FileName { get; init; }

    public string? Diagram { get; init; }

    public string? DiagramTitle { get; init; }

    public bool ShowDiagramTitleBar { get; init; } = true;

    public string? Kind { get; init; }
}

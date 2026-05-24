namespace BlogPlatform.Contracts.Posts;

public sealed class ArticleBlockDto
{
    public ArticleBlockDto(ArticleBlockType type)
    {
        Type = type;
    }

    public ArticleBlockType Type { get; }

    public string? Text { get; init; }

    public string? Summary { get; init; }

    public int Level { get; init; } = 2;

    public string? Code { get; init; }

    public string? Language { get; init; }

    public string? FileName { get; init; }

    public string? Diagram { get; init; }

    public string? DiagramTitle { get; init; }

    public bool ShowDiagramTitleBar { get; init; } = true;

    public string? Kind { get; init; }

    public ArticleTableOptionsDto TableOptions { get; init; } = new();

    public IReadOnlyList<IReadOnlyList<ArticleTableCellDto>> TableRows { get; init; } = [];
}

public sealed class ArticleTableCellDto
{
    public string? Text { get; init; }
    public string? Emoji { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageAlt { get; init; }
    public string? HorizontalAlignment { get; init; }
    public string? VerticalAlignment { get; init; }
}

public sealed class ArticleTableOptionsDto
{
    public bool HasHeaderRow { get; init; }
    public bool HasHeaderColumn { get; init; }
    public bool AutoNumberRows { get; init; }
    public string DefaultHorizontalAlignment { get; init; } = "left";
    public string DefaultVerticalAlignment { get; init; } = "middle";
}

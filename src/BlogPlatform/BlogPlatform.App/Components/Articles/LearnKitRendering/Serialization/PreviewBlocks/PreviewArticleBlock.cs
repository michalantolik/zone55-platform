namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;

public sealed class PreviewArticleBlock
{
    public PreviewArticleBlock(PreviewArticleBlockType type)
    {
        Type = type;
    }

    public PreviewArticleBlockType Type { get; }

    public string? Text { get; init; }

    public string? Summary { get; init; }

    public int Level { get; init; } = 2;

    public string? Code { get; init; }

    public string? Language { get; init; }

    public string? FileName { get; init; }

    public string? CodeTitle { get; init; }

    public bool ShowCodeTitleBar { get; init; } = true;

    public string? Diagram { get; init; }

    public string? DiagramTitle { get; init; }

    public bool ShowDiagramTitleBar { get; init; } = true;

    public string? Kind { get; init; }

    public PreviewArticleTableOptions TableOptions { get; init; } = new();

    public IReadOnlyList<IReadOnlyList<PreviewArticleTableCell>> TableRows { get; init; } = [];
}

public sealed class PreviewArticleTableCell
{
    public string? Text { get; init; }
    public string? Emoji { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageAlt { get; init; }
    public string? HorizontalAlignment { get; init; }
    public string? VerticalAlignment { get; init; }
}

public sealed class PreviewArticleTableOptions
{
    public bool HasHeaderRow { get; init; }
    public bool HasHeaderColumn { get; init; }
    public bool AutoNumberRows { get; init; }

    public string TableStyle { get; init; } = "dense-engineering";

    public string DefaultHorizontalAlignment { get; init; } = "left";

    public string DefaultVerticalAlignment { get; init; } = "middle";
}

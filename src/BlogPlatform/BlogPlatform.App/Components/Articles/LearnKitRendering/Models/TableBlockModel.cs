namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Models;

public sealed record TableBlockModel(
    bool HasHeaderRow,
    bool HasHeaderColumn,
    bool AutoNumberRows,
    string TableStyle,
    string DefaultHorizontalAlignment,
    string DefaultVerticalAlignment,
    IReadOnlyList<IReadOnlyList<TableCellModel>> Rows);

public sealed record TableCellModel(
    string Text,
    string? Emoji,
    string? ImageUrl,
    string? ImageAlt,
    string? HorizontalAlignment,
    string? VerticalAlignment);

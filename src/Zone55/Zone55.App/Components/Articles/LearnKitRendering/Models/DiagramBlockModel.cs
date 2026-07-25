namespace Zone55.App.Components.Articles.LearnKitRendering.Models;

public sealed record DiagramBlockModel(
    string Diagram,
    string? Title,
    bool ShowDiagramTitleBar,
    string DiagramType);

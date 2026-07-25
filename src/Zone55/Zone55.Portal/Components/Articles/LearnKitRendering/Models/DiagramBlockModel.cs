namespace Zone55.Portal.Components.Articles.LearnKitRendering.Models;

public sealed record DiagramBlockModel(
    string Diagram,
    string? Title,
    bool ShowDiagramTitleBar,
    string DiagramType);

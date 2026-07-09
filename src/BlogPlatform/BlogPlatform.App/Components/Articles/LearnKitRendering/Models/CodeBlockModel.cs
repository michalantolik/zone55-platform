namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Models;

public sealed record CodeBlockModel(
    string Code,
    string? Language,
    string? FileName,
    string? CodeTitle,
    bool ShowCodeTitleBar);

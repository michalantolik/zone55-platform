namespace BlogPlatform.App.Components.Articles.LearnKitRendering;

public sealed record BlockRenderFailure(
    string BlockId,
    string BlockType,
    Exception Exception);

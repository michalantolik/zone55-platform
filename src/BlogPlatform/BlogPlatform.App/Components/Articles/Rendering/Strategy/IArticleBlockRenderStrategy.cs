using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy;

public interface IArticleBlockRenderStrategy
{
    ArticleBlockType Type { get; }

    RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context);
}

using BlogPlatform.App.Components.Articles.Rendering.Strategy;
using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.App.Components.Articles.Rendering.Provider;

public interface IArticleBlockRenderStrategyProvider
{
    IArticleBlockRenderStrategy GetStrategy(ArticleBlockType type);
}

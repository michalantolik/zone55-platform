using BlogPlatform.App.Components.Articles.Rendering.Strategy;
using BlogPlatform.Contracts.Posts;

namespace BlogPlatform.App.Components.Articles.Rendering.Provider;

public sealed class ArticleBlockRenderStrategyProvider : IArticleBlockRenderStrategyProvider
{
    private readonly IReadOnlyDictionary<ArticleBlockType, IArticleBlockRenderStrategy> _strategies;

    public ArticleBlockRenderStrategyProvider(
        IEnumerable<IArticleBlockRenderStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(
            strategy => strategy.Type,
            strategy => strategy);
    }

    public IArticleBlockRenderStrategy GetStrategy(ArticleBlockType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        throw new InvalidOperationException(
            $"No article block render strategy registered for block type '{type}'.");
    }
}

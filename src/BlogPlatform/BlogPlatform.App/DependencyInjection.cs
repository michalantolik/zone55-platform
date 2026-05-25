using BlogPlatform.App.Components.Articles.Rendering.Provider;
using BlogPlatform.App.Components.Articles.Rendering.Strategy;
using BlogPlatform.App.Components.Articles.Rendering.Strategy.CallOut;
using BlogPlatform.App.Components.Articles.Rendering.Strategy.CodeBlock;
using BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;
using BlogPlatform.App.Components.Articles.Rendering.Strategy.Heading;
using BlogPlatform.App.Components.Articles.Rendering.Strategy.Markdown;
using BlogPlatform.App.Services;

namespace BlogPlatform.App;

public static class DependencyInjection
{
    public static IServiceCollection AddAppPresentation(
        this IServiceCollection services,
        HttpClient apiHttpClient)
    {
        services.AddScoped(_ => apiHttpClient);

        services.AddScoped<IBlogApiClient, BlogApiClient>();
        services.AddScoped<IRoadmapViewService, RoadmapViewService>();
        services.AddScoped<IPreviewDiagnosticsClient, PreviewDiagnosticsClient>();

        services.AddScoped<IArticleBlockRenderStrategyProvider, ArticleBlockRenderStrategyProvider>();
        services.AddScoped<IArticleBlockRenderStrategy, HeadingBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, TextBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, CodeBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, MermaidBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, PlantUmlBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, SummaryBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, TableBlockRenderStrategy>();
        services.AddScoped<IArticleBlockRenderStrategy, CalloutBlockRenderStrategy>();

        return services;
    }
}

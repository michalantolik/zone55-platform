using BlogPlatform.App.Components.Articles.LearnKitRendering.Provider;
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
        services.AddScoped<IPreviewDiagnosticsClient, PreviewDiagnosticsClient>();
        services.AddScoped<LearnKitBlockComponentTypeProvider>();

        return services;
    }
}

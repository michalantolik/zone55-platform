using BlogPlatform.App.Components.Articles.LearnKitRendering.Provider;
using BlogPlatform.App.Services;
using BlogPlatform.App.Services.LearnKit;

namespace BlogPlatform.App;

public static class DependencyInjection
{
    public static IServiceCollection AddAppPresentation(
        this IServiceCollection services,
        HttpClient apiHttpClient)
    {
        services.AddScoped(_ => apiHttpClient);

        services.AddScoped<ILearnKitApiClient, LearnKitApiClient>();
        services.AddScoped<IPreviewDiagnosticsClient, PreviewDiagnosticsClient>();
        services.AddScoped<LearnKitBlockComponentTypeProvider>();

        return services;
    }
}

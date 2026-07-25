using Zone55.App.Components.Articles.LearnKitRendering.Provider;
using Zone55.App.Services;
using Zone55.App.Services.LearnKit;

namespace Zone55.App;

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

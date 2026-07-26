using Zone55.Portal.Components.Articles.LearnKitRendering.Provider;
using Zone55.Portal.Services;
using Zone55.Portal.Services.LearnKit;

namespace Zone55.Portal;

public static class DependencyInjection
{
    public static IServiceCollection AddPortalPresentation(
        this IServiceCollection services,
        HttpClient apiHttpClient)
    {
        services.AddScoped(_ => apiHttpClient);

        services.AddScoped<ILearnKitApiClient, LearnKitApiClient>();
        services.AddScoped<IPreviewDiagnosticsClient, PreviewDiagnosticsClient>();
        services.AddScoped<LearnKitBlockComponentTypeProvider>();
        services.AddScoped<ContentLanguageState>();

        return services;
    }
}

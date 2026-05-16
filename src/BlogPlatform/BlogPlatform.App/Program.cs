using BlogPlatform.App;
using BlogPlatform.App.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("Api:BaseUrl is missing.");
}

var apiHttpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
};

builder.Services.AddScoped(_ => apiHttpClient);

builder.Logging.AddProvider(
    new ApiClientLoggerProvider(apiHttpClient));

builder.Services.AddScoped<IBlogApiClient, BlogApiClient>();
builder.Services.AddScoped<IRoadmapViewService, RoadmapViewService>();
builder.Services.AddScoped<IPreviewDiagnosticsClient, PreviewDiagnosticsClient>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogWarning("APP starting. API base URL: {ApiBaseUrl}", apiBaseUrl);

await host.RunAsync();

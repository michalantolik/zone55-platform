using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Zone55.Management.Authentication;
using Zone55.Management;
using Zone55.Management.Services;
using Zone55.Management.Localization;
using Zone55.Management.Themes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException("Api:BaseUrl is missing.");
}

var diagnosticsHttpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute) };
builder.Logging.AddProvider(new ApiClientLoggerProvider(diagnosticsHttpClient));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ManagementAuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<ManagementAuthenticationService>());
builder.Services.AddScoped<ManagementAuthorizationMessageHandler>();
builder.Services.AddHttpClient("ManagementAuthApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);
});
builder.Services.AddHttpClient("ManagementApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);
})
.AddHttpMessageHandler<ManagementAuthorizationMessageHandler>();
builder.Services.AddScoped(provider =>
    provider.GetRequiredService<IHttpClientFactory>().CreateClient("ManagementApi"));
builder.Services.AddScoped<ILearnKitManagementClient, LearnKitManagementClient>();
builder.Services.AddScoped<PreviewDiagnosticsClient>();
builder.Services.AddScoped<ClientCrashDiagnostics>();
builder.Services.AddScoped<ArticlePreviewSession>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ManagementLocalizer>();

var host = builder.Build();
await host.Services.GetRequiredService<ThemeService>().InitializeAsync();
await host.Services.GetRequiredService<ManagementLocalizer>().InitializeAsync();
var crashDiagnostics = host.Services.GetRequiredService<ClientCrashDiagnostics>();
var crashSession = await crashDiagnostics.InitializeAsync(apiBaseUrl, "MANAGEMENT_GLOBAL");
await crashDiagnostics.RecordAsync("BlazorHostBuilt", "WebAssemblyHost was built and diagnostics initialized.", new { application = "MANAGEMENT", crashSession });
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogWarning("MANAGEMENT starting. API base URL: {ApiBaseUrl}; CrashSession={CrashSession}", apiBaseUrl, crashSession);
await host.RunAsync();

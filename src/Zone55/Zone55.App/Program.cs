using Zone55.App;
using Zone55.App.Services;
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

builder.Logging.AddProvider(
    new ApiClientLoggerProvider(apiHttpClient));

builder.Services.AddAppPresentation(apiHttpClient);
builder.Services.AddScoped<ClientCrashDiagnostics>();

var host = builder.Build();

var crashDiagnostics = host.Services.GetRequiredService<ClientCrashDiagnostics>();
var crashSession = await crashDiagnostics.InitializeAsync(apiBaseUrl, "APP_GLOBAL");
await crashDiagnostics.RecordAsync("BlazorHostBuilt", "WebAssemblyHost was built and diagnostics initialized.", new { application = "APP", crashSession });
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogWarning("APP starting. API base URL: {ApiBaseUrl}; CrashSession={CrashSession}", apiBaseUrl, crashSession);

await host.RunAsync();

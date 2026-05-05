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

builder.Logging.AddProvider(new ApiClientLoggerProvider(apiBaseUrl));

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
});

builder.Services.AddScoped<IBlogApiClient, BlogApiClient>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("APP starting. API base URL: {ApiBaseUrl}", apiBaseUrl);

await host.RunAsync();

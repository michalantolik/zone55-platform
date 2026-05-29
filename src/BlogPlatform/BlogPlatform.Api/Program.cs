using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using BlogPlatform.Api;
using BlogPlatform.Api.Health;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var sharedLogFilePath = GetSharedLogFilePath();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .Enrich.WithProperty("App", "API")
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{App}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        sharedLogFilePath,
        rollingInterval: RollingInterval.Day,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{App}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("API builder created. Shared log file: {LogFilePath}", sharedLogFilePath);

AddAzureKeyVaultIfConfigured(builder.Configuration, "API");

var applicationInsightsConnectionString =
    builder.Configuration["ApplicationInsights:ConnectionString"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });

    Log.Information("API Application Insights telemetry is enabled.");
}
else
{
    Log.Information("API Application Insights telemetry is not configured.");
}

builder.Services.AddApiPresentation(builder.Configuration);
builder.Services.AddApiApplicationComposition(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeApiStorageAsync();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorApp");
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health", HealthCheckResponseWriter.AllChecks());
app.MapHealthChecks("/health/live", HealthCheckResponseWriter.ChecksByTag("live"));
app.MapHealthChecks("/health/ready", HealthCheckResponseWriter.ChecksByTag("ready"));

app.MapControllers();

Log.Information("API starting.");

app.Run();

static void AddAzureKeyVaultIfConfigured(
    ConfigurationManager configuration,
    string applicationName)
{
    var keyVaultUri = configuration["KeyVault:VaultUri"];

    if (string.IsNullOrWhiteSpace(keyVaultUri))
    {
        Log.Information("{ApplicationName} Azure Key Vault is not configured.", applicationName);
        return;
    }

    if (!Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var vaultUri))
    {
        throw new InvalidOperationException("KeyVault:VaultUri must be an absolute URI.");
    }

    configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());

    Log.Information(
        "{ApplicationName} Azure Key Vault configuration provider is enabled.",
        applicationName);
}

static string GetSharedLogFilePath()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "BlogPlatform.slnx")))
        {
            return Path.Combine(directory.FullName, "logs", "platform-log-.txt");
        }

        directory = directory.Parent;
    }

    return Path.Combine(AppContext.BaseDirectory, "logs", "platform-log-.txt");
}

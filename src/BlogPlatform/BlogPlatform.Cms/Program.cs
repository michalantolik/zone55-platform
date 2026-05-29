using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using BlogPlatform.Cms;
using BlogPlatform.Cms.Health;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var sharedLogFilePath = GetSharedLogFilePath();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Umbraco", LogEventLevel.Information)
    .Enrich.WithProperty("App", "CMS")
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

try
{
    Log.Information("CMS builder created. Shared log file: {LogFilePath}", sharedLogFilePath);

    AddAzureKeyVaultIfConfigured(builder.Configuration, "CMS");

    var applicationInsightsConnectionString =
        builder.Configuration["ApplicationInsights:ConnectionString"];

    if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = applicationInsightsConnectionString;
        });

        Log.Information("CMS Application Insights telemetry is enabled.");
    }
    else
    {
        Log.Information("CMS Application Insights telemetry is not configured.");
    }

    var hmacKey = builder.Configuration["Umbraco:CMS:Imaging:HMACSecretKey"];

    if (string.IsNullOrWhiteSpace(hmacKey))
    {
        throw new InvalidOperationException("Umbraco CMS Imaging HMACSecretKey is missing.");
    }

    if (hmacKey.StartsWith("SET_WITH", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Umbraco CMS Imaging HMACSecretKey uses a placeholder value.");
    }

    if (builder.Environment.IsDevelopment())
    {
        Log.Information("CMS development mode detected. Ensuring database exists.");

        await builder.Configuration.EnsureCmsDatabaseCreatedAsync();

        Log.Information("CMS database check completed.");
    }

    builder.Services.AddBlogPlatformCmsServices(builder.Configuration);

    builder.CreateUmbracoBuilder()
        .AddBackOffice()
        .AddWebsite()
        .AddDeliveryApi()
        .AddComposers()
        .Build();

    WebApplication app = builder.Build();

    await app.Services.InitializeCmsStorageAsync();

    Log.Information("CMS application built.");
    Log.Information("CMS environment: {EnvironmentName}", app.Environment.EnvironmentName);
    Log.Information("CMS booting Umbraco.");

    await app.BootUmbracoAsync();

    Log.Information("CMS Umbraco boot completed.");

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.MapHealthChecks("/health", HealthCheckResponseWriter.AllChecks());
    app.MapHealthChecks("/health/live", HealthCheckResponseWriter.ChecksByTag("live"));
    app.MapHealthChecks("/health/ready", HealthCheckResponseWriter.ChecksByTag("ready"));

    app.MapControllers();

    app.UseUmbraco()
        .WithMiddleware(u =>
        {
            u.UseBackOffice();
            u.UseWebsite();
        })
        .WithEndpoints(u =>
        {
            u.UseBackOfficeEndpoints();
            u.UseWebsiteEndpoints();
        });

    Log.Information("CMS starting.");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CMS terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

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

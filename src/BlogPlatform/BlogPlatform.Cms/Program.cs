using BlogPlatform.Cms;
using BlogPlatform.Cms.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var sharedLogFilePath = GetSharedLogFilePath();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .MinimumLevel.Override("Umbraco", LogEventLevel.Debug)
    .MinimumLevel.Override("Umbraco.Cms.Infrastructure.Runtime", LogEventLevel.Debug)
    .MinimumLevel.Override("Umbraco.Cms.Infrastructure.Install", LogEventLevel.Debug)
    .MinimumLevel.Override("Umbraco.Cms.Core.Runtime", LogEventLevel.Debug)
    .Enrich.WithProperty("App", "CMS")
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{App}] {SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        sharedLogFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{App}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("========== CMS STARTUP BEGIN ==========");
    Log.Information("CMS shared log file: {LogFilePath}", sharedLogFilePath);
    Log.Information("CMS content root: {ContentRoot}", builder.Environment.ContentRootPath);
    Log.Information("CMS environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("CMS application name: {ApplicationName}", builder.Environment.ApplicationName);

    LogStartupConfiguration(builder.Configuration);

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
        Log.Warning("CMS Application Insights telemetry is not configured.");
    }

    ValidateCriticalCmsConfiguration(builder.Configuration);

    if (builder.Environment.IsDevelopment())
    {
        Log.Information("CMS development mode detected. Ensuring database exists.");
        await builder.Configuration.EnsureCmsDatabaseCreatedAsync();
        Log.Information("CMS database check completed.");
    }

    builder.Services.AddBlogPlatformCmsServices(builder.Configuration);

    Log.Information("CMS registering Umbraco builder.");

    builder.CreateUmbracoBuilder()
        .AddBackOffice()
        .AddWebsite()
        .AddDeliveryApi()
        .AddComposers()
        .Build();

    Log.Information("CMS building web application.");

    WebApplication app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/health"),
        branch =>
        {
            branch.Run(async context =>
            {
                var healthCheckService =
                    context.RequestServices.GetRequiredService<HealthCheckService>();

                HealthReport report;

                if (context.Request.Path.StartsWithSegments("/health/live"))
                {
                    report = await healthCheckService.CheckHealthAsync(
                        check => check.Tags.Contains("live"),
                        context.RequestAborted);
                }
                else if (context.Request.Path.StartsWithSegments("/health/ready"))
                {
                    report = await healthCheckService.CheckHealthAsync(
                        check => check.Tags.Contains("ready"),
                        context.RequestAborted);
                }
                else
                {
                    report = await healthCheckService.CheckHealthAsync(
                        cancellationToken: context.RequestAborted);
                }

                context.Response.StatusCode =
                    report.Status == HealthStatus.Healthy
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status503ServiceUnavailable;

                await HealthCheckResponseWriter.WriteJsonAsync(context, report);
            });
        });

    app.MapControllers();

    Log.Information("CMS application built.");
    Log.Information("CMS booting Umbraco.");

    try
    {
        await app.BootUmbracoAsync();

        Log.Information("CMS Umbraco boot completed.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "CMS Umbraco boot failed.");

        await DumpUmbracoLogsAsync(app.Environment.ContentRootPath);

        throw;
    }

    Log.Information("CMS initializing BlogPlatform custom storage after Umbraco boot.");

    await app.Services.InitializeCmsStorageAsync();

    Log.Information("CMS BlogPlatform custom storage initialized.");

    app.UseHttpsRedirection();

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

    Log.Information("CMS starting web host.");
    Log.Information("========== CMS STARTUP READY ==========");

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

static void ValidateCriticalCmsConfiguration(IConfiguration configuration)
{
    RequireConfigured(configuration, "ConnectionStrings:umbracoDbDSN");
    RequireConfigured(configuration, "ConnectionStrings:umbracoDbDSN_ProviderName");
    RequireConfigured(configuration, "Umbraco:CMS:Imaging:HMACSecretKey");

    if (configuration.GetValue<bool>("Umbraco:CMS:Unattended:InstallUnattended"))
    {
        RequireConfigured(configuration, "Umbraco:CMS:Unattended:UnattendedUserName");
        RequireConfigured(configuration, "Umbraco:CMS:Unattended:UnattendedUserEmail");
        RequireConfigured(configuration, "Umbraco:CMS:Unattended:UnattendedUserPassword");
    }

    Log.Information("CMS critical configuration validation completed.");
}

static void RequireConfigured(IConfiguration configuration, string key)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Required CMS configuration key is missing: {key}");
    }

    if (value.StartsWith("SET_WITH", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Required CMS configuration key uses placeholder value: {key}");
    }
}

static void LogStartupConfiguration(IConfiguration configuration)
{
    Log.Information("CMS startup configuration snapshot:");

    LogConfigurationPresence(configuration, "ApplicationInsights:ConnectionString", secret: true);
    LogConfigurationPresence(configuration, "ConnectionStrings:umbracoDbDSN", secret: true);
    LogConfigurationPresence(configuration, "ConnectionStrings:umbracoDbDSN_ProviderName", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Global:UseHttps", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Global:InstallMissingDatabase", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Runtime:Mode", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:ModelsBuilder:ModelsMode", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Unattended:InstallUnattended", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Unattended:UpgradeUnattended", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Unattended:UnattendedUserName", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Unattended:UnattendedUserEmail", secret: false);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Unattended:UnattendedUserPassword", secret: true);
    LogConfigurationPresence(configuration, "Umbraco:CMS:Imaging:HMACSecretKey", secret: true);
}

static void LogConfigurationPresence(
    IConfiguration configuration,
    string key,
    bool secret)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value))
    {
        Log.Warning("CONFIG {Key}: MISSING", key);
        return;
    }

    if (secret)
    {
        Log.Information("CONFIG {Key}: SET length={Length}", key, value.Length);
        return;
    }

    Log.Information("CONFIG {Key}: {Value}", key, value);
}

static string GetSharedLogFilePath()
{
    var home = Environment.GetEnvironmentVariable("HOME");

    if (!string.IsNullOrWhiteSpace(home))
    {
        var azureLogFiles = Path.Combine(home, "LogFiles");

        if (Directory.Exists(azureLogFiles))
        {
            return Path.Combine(azureLogFiles, "platform-log-.txt");
        }
    }

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

static async Task DumpUmbracoLogsAsync(string contentRootPath)
{
    var logDirectories = new[]
    {
        Path.Combine(contentRootPath, "umbraco", "Logs"),
        Path.Combine(contentRootPath, "logs"),
        "/home/LogFiles"
    };

    foreach (var directory in logDirectories)
    {
        Log.Error("Checking log directory: {Directory}", directory);

        if (!Directory.Exists(directory))
        {
            Log.Error("Log directory does not exist: {Directory}", directory);
            continue;
        }

        var files = Directory
            .GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(10)
            .ToArray();

        foreach (var file in files)
        {
            Log.Error("Found log file: {File}", file);

            try
            {
                var lines = await File.ReadAllLinesAsync(file);
                var lastLines = lines.TakeLast(200);

                Log.Error("===== BEGIN LOG FILE {File} =====", file);

                foreach (var line in lastLines)
                {
                    Log.Error("{LogLine}", line);
                }

                Log.Error("===== END LOG FILE {File} =====", file);
            }
            catch (Exception readException)
            {
                Log.Error(readException, "Could not read log file: {File}", file);
            }
        }
    }
}

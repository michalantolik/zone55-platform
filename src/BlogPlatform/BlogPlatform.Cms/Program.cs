using BlogPlatform.Application.Posts;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Cms.Infrastructure.Database;
using BlogPlatform.Cms.Roadmap;
using BlogPlatform.Infrastructure;
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
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("CMS builder created. Shared log file: {LogFilePath}", sharedLogFilePath);

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

        await SqlServerDatabaseInitializer.EnsureDatabaseCreatedAsync(
            builder.Configuration);

        Log.Information("CMS database check completed.");
    }

    builder.Services.AddMemoryCache();
    builder.Services.AddControllers();

    builder.Services.AddScoped<IBlogPostQueryService, BlogPostQueryService>();
    builder.Services.AddScoped<IBlogHomeContentQueryService, BlogHomeContentQueryService>();
    builder.Services.AddScoped<IRoadmapQueryService, RoadmapQueryService>();
    builder.Services.AddScoped<IRoadmapCommandService, RoadmapCommandService>();

    builder.Services.AddInfrastructurePosts(builder.Configuration);
    builder.Services.AddSqlServerRoadmapStorage(builder.Configuration);

    builder.Services.AddScoped<
        IRoadmapArticleAssignmentChecker,
        CmsRoadmapArticleAssignmentChecker>();

    builder.CreateUmbracoBuilder()
        .AddBackOffice()
        .AddWebsite()
        .AddDeliveryApi()
        .AddComposers()
        .Build();

    WebApplication app = builder.Build();

    await app.Services.InitializeBlogPlatformDatabaseAsync();

    Log.Information("CMS application built.");
    Log.Information("CMS environment: {EnvironmentName}", app.Environment.EnvironmentName);
    Log.Information("CMS booting Umbraco.");

    await app.BootUmbracoAsync();

    Log.Information("CMS Umbraco boot completed.");

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.MapControllers();

    app.UseUmbraco()
        .WithMiddleware(u =>
        {
            Log.Information("CMS configuring Umbraco middleware.");

            u.UseBackOffice();
            u.UseWebsite();
        })
        .WithEndpoints(u =>
        {
            Log.Information("CMS configuring Umbraco endpoints.");

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

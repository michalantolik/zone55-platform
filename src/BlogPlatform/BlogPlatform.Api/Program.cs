using System.Threading.RateLimiting;
using BlogPlatform.Infrastructure;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ClientLogs", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown-client";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorApp", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

Log.Information("API application built.");
Log.Information("API environment: {EnvironmentName}", app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    Log.Information("API Swagger enabled.");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("BlazorApp");

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

Log.Information("API starting.");

app.Run();

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

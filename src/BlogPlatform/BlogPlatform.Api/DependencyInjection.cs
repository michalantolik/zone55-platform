using BlogPlatform.Api.Controllers;
using BlogPlatform.Api.Health;
using LearnKit.Application;
using LearnKit.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

namespace BlogPlatform.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.Configure<ClientLoggingOptions>(
            configuration.GetSection("ClientLogging"));

        services.AddRateLimiter(options =>
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

        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("API process is alive."),
                tags: ["live"])
            .AddCheck<LearnKitDatabaseHealthCheck>(
                "learnkit-database",
                tags: ["ready"]);

        services.AddCors(options =>
        {
            options.AddPolicy("BlazorApp", policy =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection AddApiApplicationComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLearnKitApplication();

        services.AddLearnKitInfrastructure(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Zone55Connection")));

        return services;
    }
}

using BlogPlatform.Api.Controllers;
using BlogPlatform.Api.Roadmap;
using BlogPlatform.Application;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Infrastructure;
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
        services.AddScoped<
            IRoadmapArticleAssignmentChecker,
            NoRoadmapArticleAssignmentChecker>();

        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static Task InitializeApiStorageAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        return services.InitializeBlogPlatformDatabaseAsync(cancellationToken);
    }
}

using BlogPlatform.Api.Authentication;
using BlogPlatform.Api.Controllers;
using BlogPlatform.Api.Health;
using LearnKit.Application;
using LearnKit.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

        var authSection = configuration.GetSection(LearnKitManagementAuthOptions.SectionName);
        var authOptions = authSection.Get<LearnKitManagementAuthOptions>()
            ?? throw new InvalidOperationException("LearnKit management authentication is not configured.");

        if (string.IsNullOrWhiteSpace(authOptions.Username)
            || !IsSha256Hex(authOptions.PasswordSha256)
            || string.IsNullOrWhiteSpace(authOptions.SigningKey)
            || authOptions.SigningKey.Length < 32
            || authOptions.TokenLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException(
                "LearnKit management authentication requires a username, a 64-character SHA-256 password hash, "
                + "a signing key of at least 32 characters and a positive token lifetime.");
        }

        services.Configure<LearnKitManagementAuthOptions>(authSection);
        services.AddScoped<LearnKitManagementTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(authOptions.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                LearnKitManagementAuthOptions.PolicyName,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("LearnKitManager"));
        });

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
    private static bool IsSha256Hex(string? value)
    {
        return value is { Length: 64 }
            && value.All(Uri.IsHexDigit);
    }

}

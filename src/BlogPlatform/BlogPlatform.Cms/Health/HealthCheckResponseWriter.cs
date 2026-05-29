using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlogPlatform.Cms.Health;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static Task WriteJsonAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception?.Message
            })
        };

        return httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    public static HealthCheckOptions AllChecks() => new()
    {
        ResponseWriter = WriteJsonAsync
    };

    public static HealthCheckOptions ChecksByTag(string tag) => new()
    {
        Predicate = healthCheck => healthCheck.Tags.Contains(tag),
        ResponseWriter = WriteJsonAsync
    };
}

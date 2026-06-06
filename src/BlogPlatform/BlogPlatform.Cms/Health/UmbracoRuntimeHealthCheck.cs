using Microsoft.Extensions.Diagnostics.HealthChecks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Health;

public sealed class UmbracoRuntimeHealthCheck(
    IRuntimeState runtimeState,
    IConfiguration configuration,
    ILogger<UmbracoRuntimeHealthCheck> logger) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["runtimeLevel"] = runtimeState.Level.ToString(),
            ["installUnattended"] = configuration["Umbraco:CMS:Unattended:InstallUnattended"] ?? "",
            ["upgradeUnattended"] = configuration["Umbraco:CMS:Unattended:UpgradeUnattended"] ?? "",
            ["installMissingDatabase"] = configuration["Umbraco:CMS:Global:InstallMissingDatabase"] ?? "",
            ["runtimeMode"] = configuration["Umbraco:CMS:Runtime:Mode"] ?? "",
            ["modelsMode"] = configuration["Umbraco:CMS:ModelsBuilder:ModelsMode"] ?? "",
            ["hasConnectionString"] = HasValue(configuration.GetConnectionString("umbracoDbDSN")),
            ["hasProviderName"] = HasValue(configuration.GetConnectionString("umbracoDbDSN_ProviderName")),
            ["hasHmacSecretKey"] = HasValue(configuration["Umbraco:CMS:Imaging:HMACSecretKey"]),
            ["hasUnattendedUserName"] = HasValue(configuration["Umbraco:CMS:Unattended:UnattendedUserName"]),
            ["hasUnattendedUserEmail"] = HasValue(configuration["Umbraco:CMS:Unattended:UnattendedUserEmail"]),
            ["hasUnattendedUserPassword"] = HasValue(configuration["Umbraco:CMS:Unattended:UnattendedUserPassword"])
        };

        if (runtimeState.Level == RuntimeLevel.Run)
        {
            logger.LogInformation(
                "Umbraco runtime health check healthy. Runtime level: {RuntimeLevel}",
                runtimeState.Level);

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Umbraco runtime is running.",
                    data));
        }

        logger.LogWarning(
            "Umbraco runtime health check unhealthy. Runtime level: {RuntimeLevel}. Data: {@Data}",
            runtimeState.Level,
            data);

        return Task.FromResult(
            HealthCheckResult.Unhealthy(
                $"Umbraco runtime is not ready. Runtime level: {runtimeState.Level}.",
                data: data));
    }

    private static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}

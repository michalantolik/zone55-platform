using BlogPlatform.Infrastructure.Cms;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Api.Health;

public sealed class UmbracoDeliveryApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<UmbracoDeliveryApiOptions> _options;

    public UmbracoDeliveryApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<UmbracoDeliveryApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            var client = _httpClientFactory.CreateClient("umbraco-delivery-api-health");
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(10);

            using var response = await client.GetAsync(options.PostsEndpoint, timeout.Token);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Umbraco Delivery API is reachable.")
                : HealthCheckResult.Unhealthy(
                    $"Umbraco Delivery API returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Umbraco Delivery API is not reachable.",
                ex);
        }
    }
}

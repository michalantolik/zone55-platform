using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeederHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IConfiguration _configuration;
    private readonly BlogContentSeederOptions _options;
    private readonly ILogger<BlogContentSeederHostedService> _logger;

    private CancellationTokenSource? _cts;

    public BlogContentSeederHostedService(
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime applicationLifetime,
        IConfiguration configuration,
        IOptions<BlogContentSeederOptions> options,
        ILogger<BlogContentSeederHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _applicationLifetime = applicationLifetime;
        _configuration = configuration;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Blog content seeder is disabled.");
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(() => RunSeederWhenUmbracoIsReadyAsync(_cts.Token));
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task RunSeederWhenUmbracoIsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Blog content seeder waiting for Umbraco database readiness.");

            await WaitUntilUmbracoIsReadyAsync(cancellationToken);

            using var scope = _scopeFactory.CreateScope();

            var seeder = scope.ServiceProvider.GetRequiredService<BlogContentSeeder>();

            await seeder.SeedAsync();

            _logger.LogInformation("Blog content seeder finished successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Blog content seeder was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blog content seeder failed.");
        }
    }

    private async Task WaitUntilUmbracoIsReadyAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 120;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await IsUmbracoReadyAsync(cancellationToken))
            {
                _logger.LogInformation("Umbraco is ready for blog content seeding.");
                return;
            }

            _logger.LogInformation(
                "Umbraco is not ready yet. Seeder attempt {Attempt}/{MaxAttempts}.",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        throw new TimeoutException("Umbraco database was not ready for seeding in time.");
    }

    private async Task<bool> IsUmbracoReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var runtimeState = scope.ServiceProvider.GetRequiredService<IRuntimeState>();

            if (runtimeState.Level != RuntimeLevel.Run)
            {
                return false;
            }

            return await UmbracoLockTableExistsAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> UmbracoLockTableExistsAsync(CancellationToken cancellationToken)
    {
        var connectionString =
            _configuration.GetConnectionString("umbracoDbDSN")
            ?? _configuration["Umbraco:CMS:Global:DatabaseConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT COUNT(1)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'umbracoLock'
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result) > 0;
    }
}

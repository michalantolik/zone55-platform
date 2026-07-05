using BlogPlatform.Cms.BlogContent;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogSeedImportJobService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BlogSeedImportJobService> _logger;
    private readonly object _gate = new();

    private BlogSeedImportJobSnapshot _snapshot = BlogSeedImportJobSnapshot.Idle();

    public BlogSeedImportJobService(
        IServiceScopeFactory scopeFactory,
        ILogger<BlogSeedImportJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public BlogSeedImportJobSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return _snapshot;
        }
    }

    public bool TryStart(out BlogSeedImportJobSnapshot snapshot)
    {
        lock (_gate)
        {
            if (_snapshot.Status == BlogSeedImportJobStatus.Running)
            {
                snapshot = _snapshot;
                return false;
            }

            var operationId = Guid.NewGuid().ToString("N");
            var now = DateTimeOffset.UtcNow;

            _snapshot = new BlogSeedImportJobSnapshot(
                OperationId: operationId,
                Status: BlogSeedImportJobStatus.Running,
                Success: false,
                Message: "Seed import is running.",
                Zones: 0,
                ZoneSteps: 0,
                Articles: 0,
                StartedAtUtc: now,
                CompletedAtUtc: null);

            snapshot = _snapshot;

            _ = Task.Run(() => RunAsync(operationId));

            return true;
        }
    }

    private async Task RunAsync(string operationId)
    {
        try
        {
            _logger.LogInformation(
                "Background seed import started. OperationId: {OperationId}.",
                operationId);

            using var scope = _scopeFactory.CreateScope();
            var blogContent = scope.ServiceProvider.GetRequiredService<IBlogContentAdminService>();

            var result = await blogContent.ImportSeedContentAsync(CancellationToken.None);

            SetSnapshot(operationId, new BlogSeedImportJobSnapshot(
                OperationId: operationId,
                Status: BlogSeedImportJobStatus.Succeeded,
                Success: result.Success,
                Message: result.Message,
                Zones: result.Zones,
                ZoneSteps: result.ZoneSteps,
                Articles: result.Articles,
                StartedAtUtc: GetSnapshot().StartedAtUtc,
                CompletedAtUtc: result.CompletedAtUtc));

            _logger.LogInformation(
                "Background seed import completed. OperationId: {OperationId}.",
                operationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Background seed import failed. OperationId: {OperationId}.",
                operationId);

            SetSnapshot(operationId, new BlogSeedImportJobSnapshot(
                OperationId: operationId,
                Status: BlogSeedImportJobStatus.Failed,
                Success: false,
                Message: ex.Message,
                Zones: 0,
                ZoneSteps: 0,
                Articles: 0,
                StartedAtUtc: GetSnapshot().StartedAtUtc,
                CompletedAtUtc: DateTimeOffset.UtcNow));
        }
    }

    private void SetSnapshot(string operationId, BlogSeedImportJobSnapshot snapshot)
    {
        lock (_gate)
        {
            if (_snapshot.OperationId == operationId)
            {
                _snapshot = snapshot;
            }
        }
    }
}

public static class BlogSeedImportJobStatus
{
    public const string Idle = "idle";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
}

public sealed record BlogSeedImportJobSnapshot(
    string OperationId,
    string Status,
    bool Success,
    string Message,
    int Zones,
    int ZoneSteps,
    int Articles,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc)
{
    public static BlogSeedImportJobSnapshot Idle()
    {
        return new BlogSeedImportJobSnapshot(
            OperationId: string.Empty,
            Status: BlogSeedImportJobStatus.Idle,
            Success: false,
            Message: "No seed import has been started.",
            Zones: 0,
            ZoneSteps: 0,
            Articles: 0,
            StartedAtUtc: null,
            CompletedAtUtc: null);
    }
}

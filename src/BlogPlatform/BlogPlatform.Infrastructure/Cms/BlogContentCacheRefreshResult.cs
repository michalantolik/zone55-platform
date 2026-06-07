namespace BlogPlatform.Infrastructure.Cms;

public sealed record BlogContentCacheRefreshResult(
    bool Success,
    string Message,
    int Posts,
    DateTimeOffset CompletedAtUtc);

namespace BlogPlatform.Application.Posts.Cache;

public sealed record BlogContentCacheRefreshResult(
    bool Success,
    string Message,
    int Posts,
    DateTimeOffset CompletedAtUtc);

using BlogPlatform.App.Models.LearnKit.Articles;
using BlogPlatform.App.Models.LearnKit.Roadmap;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace BlogPlatform.App.Services.LearnKit;

public sealed class LearnKitApiClient : ILearnKitApiClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<LearnKitApiClient> _logger;

    private readonly ConcurrentDictionary<string, LearnKitArticleDetailsCacheEntry> _learnKitArticleDetailsCache = [];

    public LearnKitApiClient(
        HttpClient httpClient,
        ILogger<LearnKitApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LearnKitArticleDetails?> GetLearnKitArticleBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = slug.Trim().ToLowerInvariant();

        if (_learnKitArticleDetailsCache.TryGetValue(cacheKey, out var cachedArticle) &&
            cachedArticle.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cachedArticle.Article;
        }

        var url = $"api/learnkit/articles/{Uri.EscapeDataString(slug)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _learnKitArticleDetailsCache[cacheKey] =
                    new LearnKitArticleDetailsCacheEntry(
                        null,
                        DateTimeOffset.UtcNow.Add(CacheDuration));

                return null;
            }

            response.EnsureSuccessStatusCode();

            var article =
                await response.Content.ReadFromJsonAsync<LearnKitArticleDetails>(
                    cancellationToken: cancellationToken);

            _learnKitArticleDetailsCache[cacheKey] =
                new LearnKitArticleDetailsCacheEntry(
                    article,
                    DateTimeOffset.UtcNow.Add(CacheDuration));

            return article;
        }
        catch (Exception ex) when (_learnKitArticleDetailsCache.TryGetValue(cacheKey, out var staleArticle))
        {
            _logger.LogWarning(
                ex,
                "APP failed to refresh LearnKit article from API. Returning cached article. Url: {Url}",
                url);

            return staleArticle.Article;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "APP failed to get LearnKit article from API. Url: {Url}",
                url);

            throw;
        }
    }

    public async Task<LearnKitLearningPathDetails?> GetLearnKitLearningPathByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/learnkit/roadmaps/{Uri.EscapeDataString(key)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<LearnKitLearningPathDetails>(
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "APP failed to get LearnKit learning path from API. Url: {Url}",
                url);

            throw;
        }
    }

    private sealed record LearnKitArticleDetailsCacheEntry(
        LearnKitArticleDetails? Article,
        DateTimeOffset ExpiresAt);
}

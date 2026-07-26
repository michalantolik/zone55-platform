using System.Net;
using System.Net.Http.Json;
using Backend55.Portal.Models;

namespace Backend55.Portal.Services;

public sealed class LearnKitApiClient(
    HttpClient httpClient)
{
    public async Task<LearningPathDetails?> GetPathAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            "api/learnkit/roadmaps",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<LearningPathDetails>(
                cancellationToken: cancellationToken);
    }

    public async Task<ArticleDetails?> GetArticleAsync(
        string slug,
        string languageCode = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage =
            languageCode.Trim().ToLowerInvariant();

        var url =
            $"api/learnkit/articles/{Uri.EscapeDataString(slug)}" +
            $"?language={Uri.EscapeDataString(normalizedLanguage)}";

        using var response = await httpClient.GetAsync(
            url,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ArticleDetails>(
                cancellationToken: cancellationToken);
    }
}

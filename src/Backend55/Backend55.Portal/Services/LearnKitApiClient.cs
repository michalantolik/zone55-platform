using System.Net;
using System.Net.Http.Json;
using Backend55.Portal.Models;

namespace Backend55.Portal.Services;

public sealed class LearnKitApiClient(HttpClient httpClient)
{
    public async Task<LearningPathDetails?> GetPathAsync(string key, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/learnkit/roadmaps/{Uri.EscapeDataString(key)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LearningPathDetails>(cancellationToken: cancellationToken);
    }

    public async Task<ArticleDetails?> GetArticleAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/learnkit/articles/{Uri.EscapeDataString(slug)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArticleDetails>(cancellationToken: cancellationToken);
    }
}

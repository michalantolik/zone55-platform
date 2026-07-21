using System.Net;
using System.Net.Http.Json;
using Zone55.Management.Models;

namespace Zone55.Management.Services;

public sealed class LearnKitManagementClient(HttpClient httpClient)
    : ILearnKitManagementClient
{
    public async Task<IReadOnlyCollection<ArticleManagementListItem>> GetArticlesAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ArticleManagementListItem[]>(
                "api/learnkit/admin/articles",
                cancellationToken)
            ?? [];
    }

    public async Task<ArticleManagementDetails?> GetArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/learnkit/admin/articles/{articleId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ArticleManagementDetails>(
            cancellationToken: cancellationToken);
    }

    public async Task<LearningPathManagementDetails?> GetLearningPathAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/learnkit/admin/roadmaps/{Uri.EscapeDataString(key)}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<LearningPathManagementDetails>(
            cancellationToken: cancellationToken);
    }
    public async Task UpdateArticleAsync(
        Guid articleId,
        UpdateArticleManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/articles/{articleId}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateArticleBlockAsync(
        Guid articleId,
        Guid blockId,
        UpdateArticleBlockManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/articles/{articleId}/blocks/{blockId}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task PublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"api/learnkit/admin/articles/{articleId}/publish",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UnpublishArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync(
            $"api/learnkit/admin/articles/{articleId}/unpublish",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var details = string.IsNullOrWhiteSpace(responseBody)
            ? response.ReasonPhrase
            : responseBody;

        throw new HttpRequestException(
            $"Zone55.Api returned {(int)response.StatusCode} ({response.StatusCode}). {details}");
    }

}

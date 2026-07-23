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

    public async Task UpdateLearningPathAsync(
        Guid learningPathId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/roadmaps/paths/{learningPathId}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateLearningZoneAsync(
        Guid learningZoneId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/roadmaps/zones/{learningZoneId}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateLearningStepAsync(
        Guid learningStepId,
        UpdateLearningStructureItemManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/roadmaps/steps/{learningStepId}",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<Guid> CreateArticleAsync(
        CreateArticleManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/learnkit/admin/articles",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<CreatedArticleResponse>(
            cancellationToken: cancellationToken);

        return created?.ArticleId
            ?? throw new HttpRequestException("Zone55.Api did not return the created article identifier.");
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

    public async Task DeleteArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/learnkit/admin/articles/{articleId}",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<Guid> CreateArticleBlockAsync(
        Guid articleId,
        CreateArticleBlockManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/learnkit/admin/articles/{articleId}/blocks",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<CreatedArticleBlockResponse>(
            cancellationToken: cancellationToken);

        return created?.BlockId
            ?? throw new HttpRequestException("Zone55.Api did not return the created block identifier.");
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

    public async Task DeleteArticleBlockAsync(
        Guid articleId,
        Guid blockId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/learnkit/admin/articles/{articleId}/blocks/{blockId}",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ReorderArticleBlocksAsync(
        Guid articleId,
        ReorderArticleBlocksManagementRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            $"api/learnkit/admin/articles/{articleId}/blocks/order",
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

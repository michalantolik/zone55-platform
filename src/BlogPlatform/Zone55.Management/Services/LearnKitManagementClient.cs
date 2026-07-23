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
        using var response = await GetWithStartupRetryAsync(
            "api/learnkit/admin/articles",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ArticleManagementListItem[]>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<ArticleManagementDetails?> GetArticleAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        using var response = await GetWithStartupRetryAsync(
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
        using var response = await GetWithStartupRetryAsync(
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

    public async Task<Guid> CreateLearningZoneAsync(Guid pathId, CreateLearningStructureItemManagementRequest request, CancellationToken cancellationToken = default) { using var response=await httpClient.PostAsJsonAsync($"api/learnkit/admin/roadmaps/paths/{pathId}/zones",request,cancellationToken); await EnsureSuccessAsync(response,cancellationToken); return (await response.Content.ReadFromJsonAsync<CreatedLearningStructureItemResponse>(cancellationToken:cancellationToken))?.Id ?? throw new HttpRequestException("The API did not return the created zone identifier."); }
    public async Task DeleteLearningZoneAsync(Guid pathId,Guid zoneId,CancellationToken cancellationToken=default){using var response=await httpClient.DeleteAsync($"api/learnkit/admin/roadmaps/paths/{pathId}/zones/{zoneId}",cancellationToken);await EnsureSuccessAsync(response,cancellationToken);}
    public async Task ReorderLearningZonesAsync(Guid pathId,ReorderLearningStructureItemsManagementRequest request,CancellationToken cancellationToken=default){using var response=await httpClient.PutAsJsonAsync($"api/learnkit/admin/roadmaps/paths/{pathId}/zones/order",request,cancellationToken);await EnsureSuccessAsync(response,cancellationToken);}
    public async Task<Guid> CreateLearningStepAsync(Guid zoneId,CreateLearningStructureItemManagementRequest request,CancellationToken cancellationToken=default){using var response=await httpClient.PostAsJsonAsync($"api/learnkit/admin/roadmaps/zones/{zoneId}/steps",request,cancellationToken);await EnsureSuccessAsync(response,cancellationToken);return (await response.Content.ReadFromJsonAsync<CreatedLearningStructureItemResponse>(cancellationToken:cancellationToken))?.Id ?? throw new HttpRequestException("The API did not return the created step identifier.");}
    public async Task DeleteLearningStepAsync(Guid zoneId,Guid stepId,CancellationToken cancellationToken=default){using var response=await httpClient.DeleteAsync($"api/learnkit/admin/roadmaps/zones/{zoneId}/steps/{stepId}",cancellationToken);await EnsureSuccessAsync(response,cancellationToken);}
    public async Task ReorderLearningStepsAsync(Guid zoneId,ReorderLearningStructureItemsManagementRequest request,CancellationToken cancellationToken=default){using var response=await httpClient.PutAsJsonAsync($"api/learnkit/admin/roadmaps/zones/{zoneId}/steps/order",request,cancellationToken);await EnsureSuccessAsync(response,cancellationToken);}

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

    private async Task<HttpResponseMessage> GetWithStartupRetryAsync(
        string requestUri,
        CancellationToken cancellationToken)
    {
        const int maximumAttempts = 6;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                var response = await httpClient.GetAsync(requestUri, cancellationToken);

                if (!IsTransientStartupStatusCode(response.StatusCode) || attempt == maximumAttempts)
                {
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < maximumAttempts)
            {
                // The API may still be applying migrations or seeding during a multi-project start.
            }

            await Task.Delay(GetStartupRetryDelay(attempt), cancellationToken);
        }

        throw new InvalidOperationException("The startup retry loop completed without returning a response.");
    }

    private static bool IsTransientStartupStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static TimeSpan GetStartupRetryDelay(int attempt) =>
        TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1));

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

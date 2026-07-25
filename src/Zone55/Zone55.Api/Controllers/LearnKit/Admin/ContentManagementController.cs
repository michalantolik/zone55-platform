using Zone55.Api.Authentication;
using LearnKit.Application.Content.Admin.Queries.ExportContent;
using LearnKit.Application.Content.Admin.Queries.ValidateContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Zone55.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes protected LearnKit content portability and consistency endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = LearnKitManagementAuthOptions.PolicyName)]
[Route("api/learnkit/admin/content")]
public sealed class ContentManagementController(
    ExportLearnKitContentHandler exportContent,
    ValidateLearnKitContentHandler validateContent) : ControllerBase
{
    /// <summary>
    /// Exports the complete LearnKit content graph in a deterministic order.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var export = await exportContent.HandleAsync(
            new ExportLearnKitContentQuery(),
            cancellationToken);

        var fileName = $"learnkit-content-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(export, JsonOptions),
            "application/json",
            fileName);
    }

    /// <summary>
    /// Validates identifiers, ordering and typed article block content.
    /// </summary>
    [HttpGet("validation")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        var report = await validateContent.HandleAsync(
            new ValidateLearnKitContentQuery(),
            cancellationToken);

        return Ok(report);
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

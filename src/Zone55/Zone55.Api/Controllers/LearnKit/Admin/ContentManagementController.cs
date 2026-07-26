using Zone55.Api.Authentication;
using LearnKit.Application.Content.Admin.Queries.ExportContent;
using LearnKit.Application.Content.Admin.Queries.ExportSeed;
using LearnKit.Application.Content.Admin.Queries.ValidateContent;
using LearnKit.Infrastructure.Seed;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Zone55.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes protected LearnKit content portability and consistency endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = LearnKitManagementAuthOptions.PolicyName)]
[Route("api/learnkit/admin/content")]
public sealed class ContentManagementController(
    ExportLearnKitContentHandler exportContent,
    ExportLearnKitSeedHandler exportSeed,
    ValidateLearnKitContentHandler validateContent,
    DevelopmentSeedFileWriter seedFileWriter,
    LearnKitContentSeedLoader seedLoader,
    IWebHostEnvironment environment) : ControllerBase
{
    private const string SeedFileName = "zone55-content.seed.json";
    private const string SeedRelativePath =
        "src/Zone55/Zone55.Content/Seed/Content/zone55-content.seed.json";

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
    /// Downloads content in the exact Zone55 seed-file format.
    /// </summary>
    [HttpGet("seed")]
    public async Task<IActionResult> DownloadSeed(CancellationToken cancellationToken)
    {
        var seed = await exportSeed.HandleAsync(
            new ExportLearnKitSeedQuery(),
            cancellationToken);
        var content = SerializeSeed(seed);
        await EnsureSeedCanBeLoadedAsync(content, cancellationToken);

        return File(
            content,
            "application/json",
            SeedFileName);
    }

    /// <summary>
    /// Replaces the seed file in a local repository checkout.
    /// </summary>
    [HttpPost("seed/write")]
    public async Task<IActionResult> WriteSeed(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var seed = await exportSeed.HandleAsync(
            new ExportLearnKitSeedQuery(),
            cancellationToken);
        var content = SerializeSeed(seed);
        await EnsureSeedCanBeLoadedAsync(content, cancellationToken);
        var relativePath = await seedFileWriter.WriteAsync(
            SeedRelativePath,
            content,
            cancellationToken);

        return Ok(new
        {
            fileName = SeedFileName,
            relativePath
        });
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

    private static byte[] SerializeSeed(object seed) =>
        Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(seed, JsonOptions) + "\n");

    private async Task EnsureSeedCanBeLoadedAsync(
        byte[] content,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(content, writable: false);
        await seedLoader.LoadAsync(stream, cancellationToken);
    }
}

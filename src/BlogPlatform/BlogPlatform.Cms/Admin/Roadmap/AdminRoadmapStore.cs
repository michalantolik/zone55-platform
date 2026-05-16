using System.Text.Json;
using BlogPlatform.Application.Roadmap;

namespace BlogPlatform.Cms.Admin.Roadmap;

public sealed class AdminRoadmapStore : IDotnetRoadmapStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AdminRoadmapStore(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(
            environment.ContentRootPath,
            "Admin",
            "Roadmap",
            "dotnet-roadmap.admin.json");
    }

    public async Task<DotnetRoadmap> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            await EnsureFileExistsAsync(cancellationToken);

            var json = await File.ReadAllTextAsync(
                _filePath,
                cancellationToken);

            return JsonSerializer.Deserialize<DotnetRoadmap>(json, JsonOptions())
                ?? CreateDefault();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(
        DotnetRoadmap roadmap,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            var json = JsonSerializer.Serialize(roadmap, JsonOptions());

            await File.WriteAllTextAsync(
                _filePath,
                json,
                cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureFileExistsAsync(
        CancellationToken cancellationToken)
    {
        if (File.Exists(_filePath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        var roadmap = CreateDefault();
        var json = JsonSerializer.Serialize(roadmap, JsonOptions());

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
    }

    private static DotnetRoadmap CreateDefault() =>
        DotnetRoadmapDefaults.Create();

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}

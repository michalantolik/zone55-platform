using System.Text.Json;
using BlogPlatform.Application.Roadmap;
using BlogPlatform.Domain.Entities;
using Microsoft.Extensions.Options;

namespace BlogPlatform.Infrastructure.Roadmap;

public sealed class FileSystemDotnetRoadmapStore : IDotnetRoadmapStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FileSystemDotnetRoadmapStore(
        IOptions<FileSystemRoadmapStoreOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.FilePath))
        {
            throw new InvalidOperationException("Roadmap storage file path is missing.");
        }

        _filePath = options.Value.FilePath;
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
                ?? DotnetRoadmapDefaults.Create();
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

        var json = JsonSerializer.Serialize(
            DotnetRoadmapDefaults.Create(),
            JsonOptions());

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}

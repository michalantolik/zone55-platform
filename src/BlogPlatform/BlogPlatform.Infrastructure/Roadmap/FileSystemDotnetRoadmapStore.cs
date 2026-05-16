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

            var dto = JsonSerializer.Deserialize<RoadmapDocumentDto>(
                json,
                JsonOptions());

            return dto?.ToDomain() ?? DotnetRoadmapDefaults.Create();
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

            var dto = RoadmapDocumentDto.FromDomain(roadmap);

            var json = JsonSerializer.Serialize(dto, JsonOptions());

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

        var dto = RoadmapDocumentDto.FromDomain(DotnetRoadmapDefaults.Create());

        var json = JsonSerializer.Serialize(dto, JsonOptions());

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    private sealed class RoadmapDocumentDto
    {
        public List<RoadmapZoneDto> Zones { get; set; } = [];

        public DotnetRoadmap ToDomain()
        {
            return DotnetRoadmap.Create(
                Zones
                    .OrderBy(zone => zone.Order)
                    .Select(zone => DotnetRoadmapZone.Create(
                        zone.Key,
                        zone.Name,
                        zone.Order,
                        zone.Steps
                            .OrderBy(step => step.Order)
                            .Select(step => DotnetRoadmapStep.Create(
                                step.Key,
                                step.Name,
                                step.Order)))));
        }

        public static RoadmapDocumentDto FromDomain(DotnetRoadmap roadmap)
        {
            return new RoadmapDocumentDto
            {
                Zones = roadmap.Zones
                    .OrderBy(zone => zone.Order)
                    .Select(zone => new RoadmapZoneDto
                    {
                        Key = zone.Key,
                        Name = zone.Name,
                        Order = zone.Order,
                        Steps = zone.Steps
                            .OrderBy(step => step.Order)
                            .Select(step => new RoadmapStepDto
                            {
                                Key = step.Key,
                                Name = step.Name,
                                Order = step.Order
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }
    }

    private sealed class RoadmapZoneDto
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<RoadmapStepDto> Steps { get; set; } = [];
    }

    private sealed class RoadmapStepDto
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}

using System.Text.Json;
using BlogPlatform.Contracts.DotnetRoadmap;

namespace BlogPlatform.Cms.Admin.Roadmap;

public sealed class AdminRoadmapStore
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

    public async Task<AdminRoadmapDto> GetAsync()
    {
        await _lock.WaitAsync();

        try
        {
            await EnsureFileExistsAsync();

            var json = await File.ReadAllTextAsync(_filePath);

            return JsonSerializer.Deserialize<AdminRoadmapDto>(json, JsonOptions())
                ?? CreateDefault();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(AdminRoadmapDto roadmap)
    {
        await _lock.WaitAsync();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            var json = JsonSerializer.Serialize(roadmap, JsonOptions());
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureFileExistsAsync()
    {
        if (File.Exists(_filePath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        var roadmap = CreateDefault();
        var json = JsonSerializer.Serialize(roadmap, JsonOptions());

        await File.WriteAllTextAsync(_filePath, json);
    }

    private static AdminRoadmapDto CreateDefault()
    {
        var zones = DotnetRoadmapCatalog.AllowedZoneKeys
            .Select((zoneKey, zoneIndex) => new AdminRoadmapZoneDto
            {
                Key = zoneKey,
                Name = DotnetRoadmapCatalog.ZoneDisplayNames[zoneKey],
                Order = zoneIndex + 1,
                Steps = DotnetRoadmapCatalog.ZoneStepKeys[zoneKey]
                    .Select((stepKey, stepIndex) => new AdminRoadmapStepDto
                    {
                        Key = stepKey,
                        Name = DotnetRoadmapCatalog.StepDisplayNames[stepKey],
                        Order = stepIndex + 1
                    })
                    .ToList()
            })
            .ToList();

        return new AdminRoadmapDto
        {
            Zones = zones
        };
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}

public sealed class AdminRoadmapDto
{
    public List<AdminRoadmapZoneDto> Zones { get; set; } = [];
}

public sealed class AdminRoadmapZoneDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<AdminRoadmapStepDto> Steps { get; set; } = [];
}

public sealed class AdminRoadmapStepDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

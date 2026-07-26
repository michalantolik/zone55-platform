using LearnKit.Application.Content.Admin.Contracts;
using LearnKit.Application.Content.Admin.Queries.ExportSeed;
using LearnKit.Domain.Articles;
using LearnKit.Domain.Articles.DomainModel;
using LearnKit.Domain.Articles.Entities;
using LearnKit.Domain.Roadmaps;
using LearnKit.Infrastructure;
using LearnKit.Infrastructure.Persistence;
using LearnKit.Infrastructure.Seed.Content;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace LearnKit.Infrastructure.Tests.Content;

public sealed class LearnKitContentPortabilityTests
{
    [Fact]
    public async Task ExportAsync_ShouldPreserveIdsAndUseDeterministicContentOrder()
    {
        await using var database = await TestDatabase.CreateAsync();
        var path = CreateContentGraph();
        database.Context.LearningPaths.Add(path);
        await database.Context.SaveChangesAsync();

        var store = database.Services.GetRequiredService<ILearnKitContentPortabilityStore>();
        var export = await store.ExportAsync();

        var exportedPath = Assert.Single(export.Paths);
        Assert.Equal(path.Id, exportedPath.Id);
        Assert.Equal(2, export.SchemaVersion);
        Assert.Equal(1, exportedPath.SortOrder);
        Assert.Equal(new[] { 1, 2 }, exportedPath.Zones.Select(zone => zone.SortOrder));
        Assert.Equal("foundation", exportedPath.Zones.First().Key);
        Assert.Equal("Markdown", exportedPath.Zones.First().Steps.Single().Articles.Single().Blocks.Single().Type);
    }

    [Fact]
    public async Task ExportSeedAsync_ShouldCreateASeedLoaderCompatibleDocumentWithoutDatabaseIds()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.LearningPaths.Add(CreateContentGraph());
        await database.Context.SaveChangesAsync();

        var store = database.Services.GetRequiredService<ILearnKitContentPortabilityStore>();
        var seedExport = await new ExportLearnKitSeedHandler(store).HandleAsync(
            new ExportLearnKitSeedQuery());
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var json = JsonSerializer.SerializeToUtf8Bytes(seedExport, serializerOptions);

        await using var stream = new MemoryStream(json);
        var seed = await new LearnKitContentSeedLoader().LoadAsync(stream);

        var path = Assert.Single(seed.Content.LearningPaths);
        var zone = Assert.Single(path.Zones.Where(candidate => candidate.Key == "foundation"));
        var step = Assert.Single(zone.Steps);
        var article = Assert.Single(step.Articles);
        var block = Assert.Single(article.Blocks);

        Assert.Equal("backend-cloud", path.Key);
        Assert.Equal(1, path.SortOrder);
        Assert.Equal("value-types", article.Slug);
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Equal(ArticleBlockType.Markdown, block.Type);
        Assert.Equal(2, seedExport.SchemaVersion);
        Assert.Equal("Value types", article.Translations["en"].Title);
        Assert.Equal(
            "Content",
            block.Translations["en"].Content.GetProperty("markdown").GetString());

        using var document = JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("exportedAtUtc", out _));
        Assert.False(
            document.RootElement
                .GetProperty("content")
                .GetProperty("learningPaths")[0]
                .TryGetProperty("id", out _));
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnValidReportAndAggregateCounts()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.LearningPaths.Add(CreateContentGraph());
        await database.Context.SaveChangesAsync();

        var store = database.Services.GetRequiredService<ILearnKitContentPortabilityStore>();
        var report = await store.ValidateAsync();

        Assert.True(report.IsValid);
        Assert.Empty(report.Issues);
        Assert.Equal(1, report.Counts.Paths);
        Assert.Equal(2, report.Counts.Zones);
        Assert.Equal(1, report.Counts.Steps);
        Assert.Equal(1, report.Counts.Articles);
        Assert.Equal(1, report.Counts.Blocks);
    }

    private static LearningPath CreateContentGraph()
    {
        var path = new LearningPath("backend-cloud", "Backend and cloud", "Path summary", 1);
        var cloud = new LearningZone("cloud", "Cloud", null, 2);
        var foundation = new LearningZone("foundation", "Foundation", null, 1);
        var step = new LearningStep("csharp", "C#", null, 1);
        var article = new Article(step.Id, "value-types", "Value types", 1);
        article.AddBlock(new ArticleBlock(ArticleBlockType.Markdown, 1, "{\"markdown\":\"Content\"}"));
        step.AddArticle(article);
        foundation.AddStep(step);
        path.AddZone(foundation);
        path.AddZone(cloud);
        return path;
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 7, 24, 20, 0, 0, TimeSpan.Zero);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly ServiceProvider serviceProvider;

        private TestDatabase(SqliteConnection connection, ServiceProvider serviceProvider)
        {
            this.connection = connection;
            this.serviceProvider = serviceProvider;
            Context = serviceProvider.GetRequiredService<LearnKitDbContext>();
        }

        public LearnKitDbContext Context { get; }
        public IServiceProvider Services => serviceProvider;

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var services = new ServiceCollection();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider());
            services.AddLearnKitInfrastructure(options => options.UseSqlite(connection));
            var provider = services.BuildServiceProvider();
            var context = provider.GetRequiredService<LearnKitDbContext>();
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, provider);
        }

        public async ValueTask DisposeAsync()
        {
            await serviceProvider.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}

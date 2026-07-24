using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningSteps;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningZones;
using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Domain.Articles;
using LearnKit.Domain.Roadmaps;

namespace LearnKit.Application.Tests.Roadmaps.Admin.Commands;

public sealed class ManageLearningStructureHandlersTests
{
    [Fact]
    public async Task CreateLearningZone_ShouldAppendZoneAndSave()
    {
        var path = new LearningPath("dotnet", ".NET", null);
        var store = new StoreStub(path: path);

        var id = await new CreateLearningZoneHandler(store).HandleAsync(
            new CreateLearningZoneCommand(path.Id, "cloud", "Cloud", "Cloud topics"));

        var zone = Assert.Single(path.Zones);
        Assert.Equal(id, zone.Id);
        Assert.Equal(1, zone.SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateLearningStep_ShouldAppendStepAndSave()
    {
        var zone = new LearningZone("cloud", "Cloud", null, 1);
        var store = new StoreStub(zone: zone);

        var id = await new CreateLearningStepHandler(store).HandleAsync(
            new CreateLearningStepCommand(zone.Id, "azure", "Azure", null));

        var step = Assert.Single(zone.Steps);
        Assert.Equal(id, step.Id);
        Assert.Equal(1, step.SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task ReorderLearningZones_ShouldApplyCompleteOrder()
    {
        var path = new LearningPath("dotnet", ".NET", null);
        var first = new LearningZone("first", "First", null, 1);
        var second = new LearningZone("second", "Second", null, 2);
        path.AddZone(first);
        path.AddZone(second);
        var store = new StoreStub(path: path);

        var updated = await new ReorderLearningZonesHandler(store).HandleAsync(
            new ReorderLearningZonesCommand(path.Id, [second.Id, first.Id]));

        Assert.True(updated);
        Assert.Equal([second.Id, first.Id], path.Zones.Select(zone => zone.Id));
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task ReorderLearningSteps_ShouldRejectDuplicateIdentifiers()
    {
        var zone = new LearningZone("zone", "Zone", null, 1);
        var first = new LearningStep("first", "First", null, 1);
        var second = new LearningStep("second", "Second", null, 2);
        zone.AddStep(first);
        zone.AddStep(second);
        var store = new StoreStub(zone: zone);

        var updated = await new ReorderLearningStepsHandler(store).HandleAsync(
            new ReorderLearningStepsCommand(zone.Id, [first.Id, first.Id]));

        Assert.False(updated);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteLearningStep_ShouldReturnConflict_WhenStepContainsArticles()
    {
        var zone = new LearningZone("zone", "Zone", null, 1);
        var step = new LearningStep("step", "Step", null, 1);
        step.AddArticle(new Article(step.Id, "article", "Article", 1));
        zone.AddStep(step);
        var store = new StoreStub(zone: zone);

        var result = await new DeleteLearningStepHandler(store).HandleAsync(
            new DeleteLearningStepCommand(zone.Id, step.Id));

        Assert.Equal(LearningStructureOperationResult.Conflict, result);
        Assert.Single(zone.Steps);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteLearningZone_ShouldReturnConflict_WhenZoneContainsSteps()
    {
        var path = new LearningPath("dotnet", ".NET", null);
        var zone = new LearningZone("zone", "Zone", null, 1);
        zone.AddStep(new LearningStep("step", "Step", null, 1));
        path.AddZone(zone);
        var store = new StoreStub(path: path);

        var result = await new DeleteLearningZoneHandler(store).HandleAsync(
            new DeleteLearningZoneCommand(path.Id, zone.Id));

        Assert.Equal(LearningStructureOperationResult.Conflict, result);
        Assert.Single(path.Zones);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeleteLearningStep_ShouldRemoveEmptyStepAndNormalizeOrder()
    {
        var zone = new LearningZone("zone", "Zone", null, 1);
        var first = new LearningStep("first", "First", null, 1);
        var second = new LearningStep("second", "Second", null, 2);
        zone.AddStep(first);
        zone.AddStep(second);
        var store = new StoreStub(zone: zone);

        var result = await new DeleteLearningStepHandler(store).HandleAsync(
            new DeleteLearningStepCommand(zone.Id, first.Id));

        Assert.Equal(LearningStructureOperationResult.Success, result);
        Assert.Equal(1, Assert.Single(zone.Steps).SortOrder);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    private sealed class StoreStub(
        LearningPath? path = null,
        LearningZone? zone = null,
        LearningStep? step = null) : ILearningPathManagementStore
    {
        public int SaveChangesCallCount { get; private set; }
        public Task<LearningPathManagementDetails?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<LearningPath?> GetTrackedPathByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(path?.Id == id ? path : null);
        public Task<LearningZone?> GetTrackedZoneByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(zone?.Id == id ? zone : null);
        public Task<LearningStep?> GetTrackedStepByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(step?.Id == id ? step : null);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) { SaveChangesCallCount++; return Task.CompletedTask; }
    }
}

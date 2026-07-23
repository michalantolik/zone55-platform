using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Contracts;
using LearnKit.Application.Roadmaps.Admin.Models;
using LearnKit.Domain.Roadmaps;

namespace LearnKit.Application.Tests.Roadmaps.Admin.Commands;

public sealed class UpdateLearningStructureHandlersTests
{
    [Fact]
    public async Task UpdateLearningPath_ShouldUpdateMetadataAndSave_WhenPathExists()
    {
        var path = new LearningPath("dotnet", "Old path", "Old summary");
        var store = new LearningPathManagementStoreStub(path: path);
        var handler = new UpdateLearningPathHandler(store);

        var updated = await handler.HandleAsync(
            new UpdateLearningPathCommand(path.Id, " New path ", " New summary "));

        Assert.True(updated);
        Assert.Equal("New path", path.Title);
        Assert.Equal("New summary", path.Summary);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateLearningZone_ShouldUpdateMetadataAndSave_WhenZoneExists()
    {
        var zone = new LearningZone("cloud", "Old zone", "Old summary", 1);
        var store = new LearningPathManagementStoreStub(zone: zone);
        var handler = new UpdateLearningZoneHandler(store);

        var updated = await handler.HandleAsync(
            new UpdateLearningZoneCommand(zone.Id, " New zone ", " New summary "));

        Assert.True(updated);
        Assert.Equal("New zone", zone.Title);
        Assert.Equal("New summary", zone.Summary);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateLearningStep_ShouldUpdateMetadataAndSave_WhenStepExists()
    {
        var step = new LearningStep("microservices", "Old step", "Old summary", 1);
        var store = new LearningPathManagementStoreStub(step: step);
        var handler = new UpdateLearningStepHandler(store);

        var updated = await handler.HandleAsync(
            new UpdateLearningStepCommand(step.Id, " New step ", " New summary "));

        Assert.True(updated);
        Assert.Equal("New step", step.Title);
        Assert.Equal("New summary", step.Summary);
        Assert.Equal(1, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateLearningStep_ShouldReturnFalseWithoutSaving_WhenStepDoesNotExist()
    {
        var store = new LearningPathManagementStoreStub();
        var handler = new UpdateLearningStepHandler(store);

        var updated = await handler.HandleAsync(
            new UpdateLearningStepCommand(Guid.NewGuid(), "Step", null));

        Assert.False(updated);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateLearningPath_ShouldNotSave_WhenTitleIsInvalid()
    {
        var path = new LearningPath("dotnet", "Path", "Summary");
        var store = new LearningPathManagementStoreStub(path: path);
        var handler = new UpdateLearningPathHandler(store);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(
            new UpdateLearningPathCommand(path.Id, " ", "Changed summary")));

        Assert.Equal("Path", path.Title);
        Assert.Equal("Summary", path.Summary);
        Assert.Equal(0, store.SaveChangesCallCount);
    }

    private sealed class LearningPathManagementStoreStub(
        LearningPath? path = null,
        LearningZone? zone = null,
        LearningStep? step = null) : ILearningPathManagementStore
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<LearningPathManagementDetails?> GetByKeyAsync(
            string key,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<LearningPath?> GetTrackedPathByIdAsync(
            Guid learningPathId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(path?.Id == learningPathId ? path : null);

        public Task<LearningZone?> GetTrackedZoneByIdAsync(
            Guid learningZoneId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(zone?.Id == learningZoneId ? zone : null);

        public Task<LearningStep?> GetTrackedStepByIdAsync(
            Guid learningStepId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(step?.Id == learningStepId ? step : null);

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

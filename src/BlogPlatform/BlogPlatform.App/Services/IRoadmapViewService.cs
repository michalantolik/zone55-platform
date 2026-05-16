using BlogPlatform.App.Models;

namespace BlogPlatform.App.Services;

public interface IRoadmapViewService
{
    Task<IReadOnlyCollection<LearningPathLevel>> GetDotnetRoadmapAsync(
        IReadOnlyCollection<PostListItem> posts,
        CancellationToken cancellationToken = default);
}

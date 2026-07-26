using Backend55.Portal.Models;

namespace Backend55.Portal.Services;

public sealed class LearningNavigationState(LearnKitApiClient apiClient)
{
    private LearningPathDetails? _path;
    public LearningPathDetails? Path => _path;
    public event Action? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (_path is not null) return;
        _path = await apiClient.GetPathAsync("dotnet");
        Changed?.Invoke();
    }
}

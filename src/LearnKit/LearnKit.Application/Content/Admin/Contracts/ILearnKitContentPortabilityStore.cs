using LearnKit.Application.Content.Admin.Models;

namespace LearnKit.Application.Content.Admin.Contracts;

public interface ILearnKitContentPortabilityStore
{
    Task<LearnKitContentExport> ExportAsync(CancellationToken cancellationToken = default);
    Task<LearnKitContentValidationReport> ValidateAsync(CancellationToken cancellationToken = default);
}

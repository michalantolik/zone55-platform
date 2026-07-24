using LearnKit.Application.Content.Admin.Contracts;
using LearnKit.Application.Content.Admin.Models;

namespace LearnKit.Application.Content.Admin.Queries.ExportContent;

public sealed class ExportLearnKitContentHandler(ILearnKitContentPortabilityStore store)
{
    public Task<LearnKitContentExport> HandleAsync(
        ExportLearnKitContentQuery query,
        CancellationToken cancellationToken = default) =>
        store.ExportAsync(cancellationToken);
}

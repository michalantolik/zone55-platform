using LearnKit.Application.Content.Admin.Contracts;
using LearnKit.Application.Content.Admin.Models;

namespace LearnKit.Application.Content.Admin.Queries.ValidateContent;

public sealed class ValidateLearnKitContentHandler(ILearnKitContentPortabilityStore store)
{
    public Task<LearnKitContentValidationReport> HandleAsync(
        ValidateLearnKitContentQuery query,
        CancellationToken cancellationToken = default) =>
        store.ValidateAsync(cancellationToken);
}

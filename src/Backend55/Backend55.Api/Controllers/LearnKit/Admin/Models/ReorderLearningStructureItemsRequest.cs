namespace Backend55.Api.Controllers.LearnKit.Admin.Models;
public sealed record ReorderLearningStructureItemsRequest(IReadOnlyCollection<Guid> OrderedIds);

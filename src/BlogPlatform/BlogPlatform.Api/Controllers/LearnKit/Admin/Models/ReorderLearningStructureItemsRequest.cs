namespace BlogPlatform.Api.Controllers.LearnKit.Admin.Models;
public sealed record ReorderLearningStructureItemsRequest(IReadOnlyCollection<Guid> OrderedIds);

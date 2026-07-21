namespace LearnKit.Application.Articles.Admin.Commands.ReorderArticles;
public sealed record ReorderArticlesCommand(Guid LearningStepId, IReadOnlyCollection<Guid> OrderedArticleIds);

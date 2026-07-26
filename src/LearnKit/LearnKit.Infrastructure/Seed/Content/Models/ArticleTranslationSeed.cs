using LearnKit.Domain.Articles.DomainModel;

namespace LearnKit.Infrastructure.Seed.Content.Models;

public sealed class ArticleTranslationSeed
{
    public required string Title { get; init; }

    public string Summary { get; init; } = string.Empty;

    public ArticleStatus Status { get; init; }
}

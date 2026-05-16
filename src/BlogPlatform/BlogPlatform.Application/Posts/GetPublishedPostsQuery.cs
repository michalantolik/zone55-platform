namespace BlogPlatform.Application.Posts;

public sealed record GetPublishedPostsQuery(string? CategorySlug)
{
    public static GetPublishedPostsQuery All =>
        new GetPublishedPostsQuery(CategorySlug: null);

    public bool HasCategoryFilter =>
        !string.IsNullOrWhiteSpace(CategorySlug);

    public string? NormalizedCategorySlug =>
        string.IsNullOrWhiteSpace(CategorySlug)
            ? null
            : CategorySlug.Trim();
}

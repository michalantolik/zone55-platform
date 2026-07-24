namespace Zone55.Management.Services;

public sealed class ArticlePreviewSession
{
    private ArticlePreviewDraft _draft = ArticlePreviewDraft.Empty;
    private string _mode = "Editor";
    private bool _isInitialized;
    private bool _isArticleActive;

    public event Action? Changed;

    public bool IsInitialized => _isInitialized;
    public bool IsVisible => _isArticleActive && _mode is "Split" or "Preview";
    public string TargetElementId => "persistent-article-preview-slot";
    public ArticlePreviewDraft Draft => _draft;

    public void UpdateArticle(string mode, string slug, string title, string? summary, string bodyContent)
    {
        var nextDraft = new ArticlePreviewDraft(
            Normalize(slug, "preview"),
            Normalize(title, "Untitled article"),
            summary,
            bodyContent ?? "[]");

        var changed = !_isInitialized
            || !_isArticleActive
            || !string.Equals(_mode, mode, StringComparison.Ordinal)
            || !_draft.Equals(nextDraft);

        _isInitialized = true;
        _isArticleActive = true;
        _mode = mode;
        _draft = nextDraft;

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    public void HideArticle()
    {
        if (!_isArticleActive)
        {
            return;
        }

        _isArticleActive = false;
        Changed?.Invoke();
    }

    private static string Normalize(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}

public sealed record ArticlePreviewDraft(
    string Slug,
    string Title,
    string? Summary,
    string BodyHtml)
{
    public static ArticlePreviewDraft Empty { get; } = new(
        "preview",
        "Untitled article",
        null,
        "[]");
}

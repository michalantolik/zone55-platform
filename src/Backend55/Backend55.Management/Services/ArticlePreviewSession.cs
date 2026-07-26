namespace Backend55.Management.Services;

public sealed class ArticlePreviewSession
{
    private ArticlePreviewDraft _draft = ArticlePreviewDraft.Empty;
    private string _mode = "Editor";
    private string _viewport = "Desktop";
    private bool _isExpanded;
    private bool _isInitialized;
    private bool _isArticleActive;

    public event Action? Changed;

    public bool IsInitialized => _isInitialized;
    public bool IsVisible => _isArticleActive && _mode is "Split" or "Preview";
    public bool IsExpanded => _isExpanded;
    public string Viewport => _viewport;
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

        if (changed) Changed?.Invoke();
    }

    public void SetViewport(string viewport)
    {
        if (viewport is not ("Desktop" or "Tablet" or "Phone") || string.Equals(_viewport, viewport, StringComparison.Ordinal)) return;
        _viewport = viewport;
        Changed?.Invoke();
    }

    public void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        Changed?.Invoke();
    }

    public void HideArticle()
    {
        if (!_isArticleActive) return;
        _isArticleActive = false;
        _isExpanded = false;
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
    public static ArticlePreviewDraft Empty { get; } = new("preview", "Untitled article", null, "[]");
}

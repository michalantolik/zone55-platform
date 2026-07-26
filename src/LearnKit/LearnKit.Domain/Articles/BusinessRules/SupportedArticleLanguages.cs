namespace LearnKit.Domain.Articles.BusinessRules;

/// <summary>
/// Defines languages supported by LearnKit articles.
/// </summary>
public static class SupportedArticleLanguages
{
    public const string Polish = "pl";
    public const string English = "en";
    public const string German = "de";

    public const string Default = English;

    public static IReadOnlyCollection<string> All { get; } =
    [
        Polish,
        English,
        German
    ];

    public static bool IsSupported(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        return All.Contains(
            languageCode.Trim(),
            StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string languageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        var normalized = languageCode.Trim().ToLowerInvariant();

        if (!IsSupported(normalized))
        {
            throw new ArgumentException(
                $"Unsupported article language '{languageCode}'.",
                nameof(languageCode));
        }

        return normalized;
    }
}

namespace BlogPlatform.App.Components.Articles.Shared;

public static class CodeBlockRenderer
{
    public static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "text";
        }

        return language.Trim().ToLowerInvariant() switch
        {
            "cs" => "csharp",
            "cshtml" => "razor",
            "html" => "markup",
            "htm" => "markup",
            "yml" => "yaml",
            "terraform" => "hcl",
            "tf" => "hcl",
            "ps1" => "powershell",
            "shell" => "bash",
            "sh" => "bash",
            _ => language.Trim().ToLowerInvariant()
        };
    }

    public static string GetCodeTitle(
        string? codeTitle,
        string? fileName,
        string? language)
    {
        if (!string.IsNullOrWhiteSpace(codeTitle))
        {
            return codeTitle;
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            return language;
        }

        return "Code snippet";
    }

    public static IReadOnlyCollection<CodeLine> GetCodeLines(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return [];
        }

        return code
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select((text, index) => new CodeLine(index + 1, text))
            .ToArray();
    }

    public sealed record CodeLine(int Number, string Text);
}

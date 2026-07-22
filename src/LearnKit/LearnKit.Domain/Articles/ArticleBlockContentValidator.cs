using System.Text.Json;

namespace LearnKit.Domain.Articles;

/// <summary>
/// Validates the JSON contract used by each LearnKit article block type.
/// </summary>
public static class ArticleBlockContentValidator
{
    public static void Validate(
        ArticleBlockType blockType,
        string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
        {
            throw new ArticleBlockContentValidationException(
                blockType,
                ["Block content cannot be empty."]);
        }

        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(contentJson);
        }
        catch (JsonException exception)
        {
            throw new ArticleBlockContentValidationException(
                blockType,
                [$"Block content must be valid JSON: {exception.Message}"]);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArticleBlockContentValidationException(
                    blockType,
                    ["Block content must be a JSON object."]);
            }

            var errors = new List<string>();
            var content = document.RootElement;

            switch (blockType)
            {
                case ArticleBlockType.Markdown:
                    RequireAnyNonEmptyString(content, errors, "markdown", "text");
                    break;
                case ArticleBlockType.Code:
                    RequireNonEmptyString(content, errors, "code");
                    ValidateOptionalString(content, errors, "language");
                    ValidateOptionalString(content, errors, "fileName");
                    ValidateOptionalString(content, errors, "codeTitle");
                    ValidateOptionalBoolean(content, errors, "showCodeTitleBar");
                    break;
                case ArticleBlockType.Diagram:
                    RequireAnyNonEmptyString(content, errors, "diagram", "plantUml", "mermaid");
                    ValidateOptionalString(content, errors, "diagramType");
                    ValidateOptionalString(content, errors, "title");
                    ValidateOptionalBoolean(content, errors, "showDiagramTitleBar");
                    break;
                case ArticleBlockType.Table:
                    ValidateTable(content, errors);
                    break;
                case ArticleBlockType.Callout:
                    RequireNonEmptyString(content, errors, "text");
                    ValidateOptionalString(content, errors, "kind");
                    break;
                case ArticleBlockType.Summary:
                    RequireAnyNonEmptyString(content, errors, "summary", "text");
                    break;
                default:
                    errors.Add($"Article block type '{blockType}' is not supported.");
                    break;
            }

            if (errors.Count > 0)
            {
                throw new ArticleBlockContentValidationException(blockType, errors);
            }
        }
    }

    private static void ValidateTable(JsonElement content, ICollection<string> errors)
    {
        if (!content.TryGetProperty("rows", out var rows))
        {
            errors.Add("Property 'rows' is required.");
        }
        else if (rows.ValueKind != JsonValueKind.Array)
        {
            errors.Add("Property 'rows' must be an array.");
        }
        else
        {
            var rowIndex = 0;
            foreach (var row in rows.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array)
                {
                    errors.Add($"Table row {rowIndex + 1} must be an array.");
                }

                rowIndex++;
            }
        }

        ValidateOptionalBoolean(content, errors, "hasHeaderRow");
        ValidateOptionalBoolean(content, errors, "hasHeaderColumn");
        ValidateOptionalBoolean(content, errors, "autoNumberRows");
        ValidateOptionalString(content, errors, "tableStyle");
        ValidateOptionalString(content, errors, "defaultHorizontalAlignment");
        ValidateOptionalString(content, errors, "defaultVerticalAlignment");
    }

    private static void RequireAnyNonEmptyString(
        JsonElement content,
        ICollection<string> errors,
        params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetNonEmptyString(content, propertyName))
            {
                return;
            }
        }

        errors.Add($"One of the following properties is required and must contain text: {string.Join(", ", propertyNames.Select(name => $"'{name}'"))}.");
    }

    private static void RequireNonEmptyString(
        JsonElement content,
        ICollection<string> errors,
        string propertyName)
    {
        if (!TryGetNonEmptyString(content, propertyName))
        {
            errors.Add($"Property '{propertyName}' is required and must contain text.");
        }
    }

    private static bool TryGetNonEmptyString(JsonElement content, string propertyName)
    {
        return content.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString());
    }

    private static void ValidateOptionalString(
        JsonElement content,
        ICollection<string> errors,
        string propertyName)
    {
        if (content.TryGetProperty(propertyName, out var property)
            && property.ValueKind is not JsonValueKind.String and not JsonValueKind.Null)
        {
            errors.Add($"Property '{propertyName}' must be a string when provided.");
        }
    }

    private static void ValidateOptionalBoolean(
        JsonElement content,
        ICollection<string> errors,
        string propertyName)
    {
        if (content.TryGetProperty(propertyName, out var property)
            && property.ValueKind is not JsonValueKind.True and not JsonValueKind.False and not JsonValueKind.Null)
        {
            errors.Add($"Property '{propertyName}' must be a boolean when provided.");
        }
    }
}

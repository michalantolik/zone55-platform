using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogPlatform.Cms.Seeding.Blocks;

public static class SeedBlockJson
{
    public static readonly JsonSerializerOptions TableRowsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    public static int? GetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt32(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), out number))
        {
            return number;
        }

        return null;
    }

    public static bool? GetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(property.GetString(), out var value) => value,
            _ => null
        };
    }

    public static string NormalizeTableStyle(string? value)
    {
        return string.Equals(value, "minimal-reference", StringComparison.OrdinalIgnoreCase)
            ? "minimal-reference"
            : "dense-engineering";
    }

    public static List<List<BlogSeedTableCell>> GetTableRows(JsonElement blockElement)
    {
        if (!blockElement.TryGetProperty("rows", out var rowsProperty))
        {
            return [];
        }

        if (rowsProperty.ValueKind == JsonValueKind.String)
        {
            var rawRowsJson = rowsProperty.GetString();

            if (string.IsNullOrWhiteSpace(rawRowsJson))
            {
                return [];
            }

            try
            {
                using var document = JsonDocument.Parse(rawRowsJson);

                return ParseTableRows(document.RootElement);
            }
            catch
            {
                return [];
            }
        }

        return ParseTableRows(rowsProperty);
    }

    public static void NormalizeTableRowsForUmbraco(Dictionary<string, object?> block)
    {
        if (!block.TryGetValue("rows", out var rowsValue) || rowsValue is null)
        {
            block["rows"] = "[]";
            return;
        }

        if (rowsValue is JsonElement rowsElement)
        {
            block["rows"] = rowsElement.ValueKind == JsonValueKind.String
                ? rowsElement.GetString() ?? "[]"
                : rowsElement.GetRawText();

            return;
        }

        if (rowsValue is string rowsText)
        {
            block["rows"] = string.IsNullOrWhiteSpace(rowsText)
                ? "[]"
                : rowsText;

            return;
        }

        block["rows"] = JsonSerializer.Serialize(
            rowsValue,
            TableRowsJsonOptions);
    }

    private static List<List<BlogSeedTableCell>> ParseTableRows(JsonElement rowsElement)
    {
        if (rowsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return rowsElement
            .EnumerateArray()
            .Where(row => row.ValueKind == JsonValueKind.Array)
            .Select(row => row
                .EnumerateArray()
                .Select(ParseTableCell)
                .ToList())
            .ToList();
    }

    private static BlogSeedTableCell ParseTableCell(JsonElement cellElement)
    {
        if (cellElement.ValueKind == JsonValueKind.String)
        {
            return new BlogSeedTableCell
            {
                Text = cellElement.GetString()
            };
        }

        if (cellElement.ValueKind != JsonValueKind.Object)
        {
            return new BlogSeedTableCell();
        }

        return new BlogSeedTableCell
        {
            Text = GetString(cellElement, "text"),
            Emoji = GetString(cellElement, "emoji"),
            ImageUrl = GetString(cellElement, "imageUrl"),
            ImageAlt = GetString(cellElement, "imageAlt"),
            HorizontalAlignment = GetString(cellElement, "horizontalAlignment"),
            VerticalAlignment = GetString(cellElement, "verticalAlignment")
        };
    }
}

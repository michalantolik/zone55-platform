using BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization.PreviewBlocks;
using BlogPlatform.App.Models.LearnKit;
using BlogPlatform.App.Models.LearnKit.Articles;
using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization;

/// <summary>
/// Converts a Zone55 Management live-preview payload into the article model rendered by the App.
/// </summary>
public static class LearnKitArticlePreviewMapper
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public static LearnKitArticleDetails ToArticle(
        LearnKitArticlePreviewPayload preview)
    {
        return new LearnKitArticleDetails
        {
            Id = $"preview-{preview.Slug}",
            Slug = preview.Slug,
            Title = preview.Title,
            Summary = preview.Summary,
            Status = "Preview",
            Blocks = ParseBlocks(preview.BodyContent)
        };
    }

    private static IReadOnlyList<LearnKitArticleBlockDetails> ParseBlocks(
        string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(body);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return ParseLearnKitBlocks(document.RootElement);
            }

            return PreviewArticleBlockParser.Parse(body)
                .Select((block, index) => ToLearnKitBlock(block, index + 1))
                .ToArray();
        }
        catch (JsonException)
        {
            return
            [
                new LearnKitArticleBlockDetails
                {
                    Id = "preview-block-1",
                    Type = LearnKitBlockTypes.Markdown,
                    SortOrder = 1,
                    ContentJson = JsonSerializer.Serialize(
                        new
                        {
                            markdown = body
                        },
                        JsonOptions)
                }
            ];
        }
    }

    private static IReadOnlyList<LearnKitArticleBlockDetails> ParseLearnKitBlocks(
        JsonElement root)
    {
        var blocks = new List<LearnKitArticleBlockDetails>();
        var index = 0;

        foreach (var element in root.EnumerateArray())
        {
            index++;

            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty("type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var type = typeElement.GetString();

            if (string.IsNullOrWhiteSpace(type))
            {
                continue;
            }

            var sortOrder =
                element.TryGetProperty("sortOrder", out var sortOrderElement) &&
                sortOrderElement.TryGetInt32(out var parsedSortOrder)
                    ? parsedSortOrder
                    : index;

            blocks.Add(new LearnKitArticleBlockDetails
            {
                Id = $"preview-block-{index}",
                Type = type,
                SortOrder = sortOrder,
                ContentJson = BuildContentJson(element)
            });
        }

        return blocks
            .OrderBy(block => block.SortOrder)
            .ThenBy(block => block.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildContentJson(JsonElement element)
    {
        var content =
            new Dictionary<string, JsonElement>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals("type") ||
                property.NameEquals("sortOrder"))
            {
                continue;
            }

            content[property.Name] = property.Value.Clone();
        }

        return JsonSerializer.Serialize(content, JsonOptions);
    }

    private static LearnKitArticleBlockDetails ToLearnKitBlock(
        PreviewArticleBlock block,
        int sortOrder)
    {
        var type = GetLearnKitBlockType(block);

        return new LearnKitArticleBlockDetails
        {
            Id = $"preview-block-{sortOrder}",
            Type = type,
            SortOrder = sortOrder,
            ContentJson = JsonSerializer.Serialize(
                CreateContent(block),
                JsonOptions)
        };
    }

    private static string GetLearnKitBlockType(
        PreviewArticleBlock block)
    {
        return block.Type switch
        {
            PreviewArticleBlockType.Code =>
                LearnKitBlockTypes.Code,

            PreviewArticleBlockType.PlantUml or
            PreviewArticleBlockType.Mermaid =>
                LearnKitBlockTypes.Diagram,

            PreviewArticleBlockType.Table =>
                LearnKitBlockTypes.Table,

            PreviewArticleBlockType.Callout =>
                LearnKitBlockTypes.Callout,

            PreviewArticleBlockType.Summary =>
                LearnKitBlockTypes.Summary,

            _ =>
                LearnKitBlockTypes.Markdown
        };
    }

    private static object CreateContent(PreviewArticleBlock block)
    {
        return block.Type switch
        {
            PreviewArticleBlockType.Heading => new
            {
                type = "heading",
                level = block.Level,
                text = block.Text,
                markdown = ToHeadingMarkdown(
                    block.Level,
                    block.Text),
                sourceType = "heading"
            },

            PreviewArticleBlockType.Text => new
            {
                type = "text",
                text = block.Text,
                markdown = block.Text,
                sourceType = "text"
            },

            PreviewArticleBlockType.Summary => new
            {
                type = "summary",
                summary = block.Summary ?? block.Text,
                sourceType = "summary"
            },

            PreviewArticleBlockType.Code => new
            {
                type = "codeSnippet",
                code = block.Code,
                language = block.Language,
                fileName = block.FileName,
                codeTitle = block.CodeTitle,
                showCodeTitleBar = block.ShowCodeTitleBar,
                sourceType = "codeSnippet"
            },

            PreviewArticleBlockType.PlantUml => new
            {
                type = "plantUmlDiagram",
                diagram = block.Diagram,
                title = block.DiagramTitle,
                showDiagramTitleBar = block.ShowDiagramTitleBar,
                diagramType = "PlantUml",
                sourceType = "plantUmlDiagram"
            },

            PreviewArticleBlockType.Mermaid => new
            {
                type = "mermaidDiagram",
                diagram = block.Diagram,
                title = block.DiagramTitle,
                showDiagramTitleBar = block.ShowDiagramTitleBar,
                diagramType = "Mermaid",
                sourceType = "mermaidDiagram"
            },

            PreviewArticleBlockType.Table => new
            {
                type = "table",
                rows = block.TableRows
                    .Select(row => row
                        .Select(cell => new
                        {
                            text = cell.Text,
                            emoji = cell.Emoji,
                            imageUrl = cell.ImageUrl,
                            imageAlt = cell.ImageAlt,
                            horizontalAlignment =
                                cell.HorizontalAlignment,
                            verticalAlignment =
                                cell.VerticalAlignment
                        })
                        .ToArray())
                    .ToArray(),
                hasHeaderRow =
                    block.TableOptions.HasHeaderRow,
                hasHeaderColumn =
                    block.TableOptions.HasHeaderColumn,
                autoNumberRows =
                    block.TableOptions.AutoNumberRows,
                tableStyle =
                    block.TableOptions.TableStyle,
                defaultHorizontalAlignment =
                    block.TableOptions.DefaultHorizontalAlignment,
                defaultVerticalAlignment =
                    block.TableOptions.DefaultVerticalAlignment,
                sourceType = "table"
            },

            PreviewArticleBlockType.Callout => new
            {
                type = "callout",
                kind = block.Kind,
                text = block.Text,
                sourceType = "callout"
            },

            _ => new
            {
                type = "text",
                text = block.Text,
                markdown = block.Text,
                sourceType = "text"
            }
        };
    }

    private static string ToHeadingMarkdown(
        int level,
        string? text)
    {
        var normalizedLevel = Math.Clamp(level, 1, 6);

        return $"{new string('#', normalizedLevel)} {text}";
    }
}

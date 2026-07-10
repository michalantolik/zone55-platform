using BlogPlatform.App.Models.LearnKit.Articles;
using BlogPlatform.Contracts.Posts;
using System.Text.Json;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering.Serialization;

/// <summary>
/// Converts the CMS live-preview payload into the article model rendered by the App.
/// </summary>
public static class LearnKitArticlePreviewMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static LearnKitArticleDetails ToArticle(LearnKitArticlePreviewPayload preview)
    {
        var blocks = ParseBlocks(preview.BodyContent)
            .Select((block, index) => ToLearnKitBlock(block, index + 1))
            .ToArray();

        return new LearnKitArticleDetails
        {
            Id = $"preview-{preview.Slug}",
            Slug = preview.Slug,
            Title = preview.Title,
            Summary = preview.Summary,
            Status = "Preview",
            Blocks = blocks
        };
    }

    private static IReadOnlyCollection<ArticleBlockDto> ParseBlocks(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        try
        {
            return ArticleBlockParser.Parse(body);
        }
        catch (JsonException)
        {
            return [new ArticleBlockDto(ArticleBlockType.Text)
            {
                Text = body
            }];
        }
    }

    private static LearnKitArticleBlockDetails ToLearnKitBlock(
        ArticleBlockDto block,
        int sortOrder)
    {
        var type = GetLearnKitBlockType(block);

        return new LearnKitArticleBlockDetails
        {
            Id = $"preview-block-{sortOrder}",
            Type = type,
            SortOrder = sortOrder,
            ContentJson = JsonSerializer.Serialize(CreateContent(block), JsonOptions)
        };
    }

    private static string GetLearnKitBlockType(ArticleBlockDto block)
    {
        return block.Type switch
        {
            ArticleBlockType.Code => LearnKitBlockTypes.Code,
            ArticleBlockType.PlantUml or ArticleBlockType.Mermaid => LearnKitBlockTypes.Diagram,
            ArticleBlockType.Table => LearnKitBlockTypes.Table,
            ArticleBlockType.Callout => LearnKitBlockTypes.Callout,
            ArticleBlockType.Summary => LearnKitBlockTypes.Summary,
            _ => LearnKitBlockTypes.Markdown
        };
    }

    private static object CreateContent(ArticleBlockDto block)
    {
        return block.Type switch
        {
            ArticleBlockType.Heading => new
            {
                type = "heading",
                level = block.Level,
                text = block.Text,
                markdown = ToHeadingMarkdown(block.Level, block.Text),
                sourceType = "heading"
            },

            ArticleBlockType.Text => new
            {
                type = "text",
                text = block.Text,
                markdown = block.Text,
                sourceType = "text"
            },

            ArticleBlockType.Summary => new
            {
                type = "summary",
                summary = block.Summary ?? block.Text,
                sourceType = "summary"
            },

            ArticleBlockType.Code => new
            {
                type = "codeSnippet",
                code = block.Code,
                language = block.Language,
                fileName = block.FileName,
                codeTitle = block.CodeTitle,
                showCodeTitleBar = block.ShowCodeTitleBar,
                sourceType = "codeSnippet"
            },

            ArticleBlockType.PlantUml => new
            {
                type = "plantUmlDiagram",
                diagram = block.Diagram,
                title = block.DiagramTitle,
                showDiagramTitleBar = block.ShowDiagramTitleBar,
                diagramType = "PlantUml",
                sourceType = "plantUmlDiagram"
            },

            ArticleBlockType.Mermaid => new
            {
                type = "mermaidDiagram",
                diagram = block.Diagram,
                title = block.DiagramTitle,
                showDiagramTitleBar = block.ShowDiagramTitleBar,
                diagramType = "Mermaid",
                sourceType = "mermaidDiagram"
            },

            ArticleBlockType.Table => new
            {
                type = "table",
                rows = block.TableRows.Select(row => row.Select(cell => new
                {
                    text = cell.Text,
                    emoji = cell.Emoji,
                    imageUrl = cell.ImageUrl,
                    imageAlt = cell.ImageAlt,
                    horizontalAlignment = cell.HorizontalAlignment,
                    verticalAlignment = cell.VerticalAlignment
                }).ToArray()).ToArray(),
                hasHeaderRow = block.TableOptions.HasHeaderRow,
                hasHeaderColumn = block.TableOptions.HasHeaderColumn,
                autoNumberRows = block.TableOptions.AutoNumberRows,
                tableStyle = block.TableOptions.TableStyle,
                defaultHorizontalAlignment = block.TableOptions.DefaultHorizontalAlignment,
                defaultVerticalAlignment = block.TableOptions.DefaultVerticalAlignment,
                sourceType = "table"
            },

            ArticleBlockType.Callout => new
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

    private static string ToHeadingMarkdown(int level, string? text)
    {
        var normalizedLevel = Math.Clamp(level, 1, 6);

        return $"{new string('#', normalizedLevel)} {text}";
    }
}

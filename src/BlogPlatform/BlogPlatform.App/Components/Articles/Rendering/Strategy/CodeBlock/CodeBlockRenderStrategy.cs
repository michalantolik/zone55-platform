using BlogPlatform.App.Components.Articles.Rendering.Context;
using BlogPlatform.Contracts.Posts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.CodeBlock;

public sealed class CodeBlockRenderStrategy : IArticleBlockRenderStrategy
{
    public ArticleBlockType Type => ArticleBlockType.Code;

    public RenderFragment Render(
        ArticleBlockDto block,
        ArticleBlockRenderContext context) => builder =>
        {
            var sequence = 0;
            var title = GetCodeTitle(block);
            var showTitleBar = block.ShowCodeTitleBar;

            builder.OpenElement(sequence++, "figure");
            builder.AddAttribute(
                sequence++,
                "class",
                showTitleBar ? "code-block" : "code-block code-block-title-hidden");

            if (showTitleBar)
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "snippet-toolbar");

                builder.OpenElement(sequence++, "figcaption");
                builder.AddContent(sequence++, title);
                builder.CloseElement();

                RenderCopyButton(builder, ref sequence, block, context, "copy-button");

                builder.CloseElement();
            }
            else
            {
                RenderCopyButton(builder, ref sequence, block, context, "copy-button copy-button-floating");
            }

            builder.OpenElement(sequence++, "pre");
            builder.OpenElement(sequence++, "code");
            builder.AddAttribute(sequence++, "class", $"language-{block.Language}");

            foreach (var line in GetCodeLines(block.Code))
            {
                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "code-line");

                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "line-number");
                builder.AddContent(sequence++, line.Number);
                builder.CloseElement();

                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "line-content");
                builder.AddContent(sequence++, line.Text);
                builder.CloseElement();

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();
        };

    private static void RenderCopyButton(
        RenderTreeBuilder builder,
        ref int sequence,
        ArticleBlockDto block,
        ArticleBlockRenderContext context,
        string cssClass)
    {
        builder.OpenElement(sequence++, "button");
        builder.AddAttribute(sequence++, "type", "button");
        builder.AddAttribute(sequence++, "class", cssClass);
        builder.AddAttribute(
            sequence++,
            "onclick",
            EventCallback.Factory.Create(
                context.EventReceiver,
                () => context.CopyToClipboardAsync(block.Code)));
        builder.AddContent(sequence++, "Copy");
        builder.CloseElement();
    }

    private static string GetCodeTitle(ArticleBlockDto block)
    {
        if (!string.IsNullOrWhiteSpace(block.CodeTitle))
        {
            return block.CodeTitle;
        }

        if (!string.IsNullOrWhiteSpace(block.FileName))
        {
            return block.FileName;
        }

        if (!string.IsNullOrWhiteSpace(block.Language))
        {
            return block.Language;
        }

        return "Code snippet";
    }

    private static IReadOnlyCollection<CodeLine> GetCodeLines(string? code)
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

    private sealed record CodeLine(int Number, string Text);
}

using LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlockTranslation;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticleTranslation;
using LearnKit.Domain.Articles.BusinessRules;
using LearnKit.Domain.Articles.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zone55.Api.Authentication;
using Zone55.Api.Controllers.LearnKit.Admin.Models;

namespace Zone55.Api.Controllers.LearnKit.Admin;

/// <summary>
/// Exposes management endpoints for article translations.
/// </summary>
[ApiController]
[Authorize(Policy = LearnKitManagementAuthOptions.PolicyName)]
[Route("api/learnkit/admin/articles/{articleId:guid}")]
public sealed class ArticleTranslationsManagementController
    : ControllerBase
{
    private readonly UpdateArticleTranslationHandler
        _updateArticleTranslationHandler;

    private readonly UpdateArticleBlockTranslationHandler
        _updateArticleBlockTranslationHandler;

    public ArticleTranslationsManagementController(
        UpdateArticleTranslationHandler updateArticleTranslationHandler,
        UpdateArticleBlockTranslationHandler
            updateArticleBlockTranslationHandler)
    {
        _updateArticleTranslationHandler =
            updateArticleTranslationHandler;

        _updateArticleBlockTranslationHandler =
            updateArticleBlockTranslationHandler;
    }

    /// <summary>
    /// Creates or updates one language version of an article.
    /// </summary>
    [HttpPut("translations/{languageCode}")]
    public async Task<IActionResult> UpdateArticleTranslation(
        Guid articleId,
        string languageCode,
        [FromBody] UpdateArticleTranslationRequest request,
        CancellationToken cancellationToken)
    {
        if (!SupportedArticleLanguages.IsSupported(languageCode))
        {
            return UnsupportedLanguage(languageCode);
        }

        var command = new UpdateArticleTranslationCommand(
            articleId,
            languageCode,
            request.Title,
            request.Summary);

        var updated =
            await _updateArticleTranslationHandler.HandleAsync(
                command,
                cancellationToken);

        if (!updated)
        {
            return ManagementErrors.NotFound(
                "article_not_found",
                "The requested article does not exist.");
        }

        return NoContent();
    }

    /// <summary>
    /// Creates or updates one language version
    /// of an article block.
    /// </summary>
    [HttpPut(
        "blocks/{blockId:guid}/translations/{languageCode}")]
    public async Task<IActionResult> UpdateBlockTranslation(
        Guid articleId,
        Guid blockId,
        string languageCode,
        [FromBody] UpdateArticleBlockTranslationRequest request,
        CancellationToken cancellationToken)
    {
        if (!SupportedArticleLanguages.IsSupported(languageCode))
        {
            return UnsupportedLanguage(languageCode);
        }

        var command = new UpdateArticleBlockTranslationCommand(
            articleId,
            blockId,
            languageCode,
            request.ContentJson);

        bool updated;

        try
        {
            updated =
                await _updateArticleBlockTranslationHandler.HandleAsync(
                    command,
                    cancellationToken);
        }
        catch (InvalidArticleBlockException exception)
        {
            return InvalidBlockContent(exception);
        }

        if (!updated)
        {
            return ManagementErrors.NotFound(
                "article_or_block_not_found",
                "The requested article or block does not exist.");
        }

        return NoContent();
    }

    private ActionResult UnsupportedLanguage(
        string languageCode)
    {
        return ManagementErrors.BadRequest(
            "article_language_unsupported",
            $"Unsupported article language '{languageCode}'.");
    }

    private ActionResult InvalidBlockContent(
        InvalidArticleBlockException exception)
    {
        ModelState.AddModelError(
            "contentJson",
            exception.Message);

        foreach (var error in exception.Errors)
        {
            ModelState.AddModelError(
                "contentJson",
                error);
        }

        return ValidationProblem(ModelState);
    }
}

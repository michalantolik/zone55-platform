using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningZone;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningZones;
using LearnKit.Application.Roadmaps.Admin.Commands.CreateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.DeleteLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.ReorderLearningSteps;
using LearnKit.Application.Articles.Admin.Commands.CreateArticle;
using LearnKit.Application.Articles.Admin.Commands.CreateArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.DeleteArticle;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticles;
using LearnKit.Application.Roadmaps.Admin.Queries.GetLearningPathForManagement;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningPath;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningStep;
using LearnKit.Application.Roadmaps.Admin.Commands.UpdateLearningZone;
using LearnKit.Application.Articles.Admin.Commands.ReorderArticleBlocks;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticleBlock;
using LearnKit.Application.Articles.Admin.Commands.PublishArticle;
using LearnKit.Application.Articles.Admin.Commands.UnpublishArticle;
using LearnKit.Application.Articles.Admin.Commands.UpdateArticle;
using LearnKit.Application.Articles.Admin.Queries.GetArticleForEditing;
using LearnKit.Application.Articles.Admin.Queries.GetArticlesForManagement;
using LearnKit.Application.Articles.Public.Queries.GetArticleBySlug;
using LearnKit.Application.Roadmaps.Public.Queries.GetLearningPath;
using Microsoft.Extensions.DependencyInjection;

namespace LearnKit.Application;

/// <summary>
/// Registers LearnKit application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all LearnKit application services.
    /// </summary>
    public static IServiceCollection AddLearnKitApplication(
        this IServiceCollection services)
    {
        services.AddScoped<GetArticleBySlugHandler>();
        services.AddScoped<GetLearningPathHandler>();

        services.AddScoped<GetArticlesForManagementHandler>();
        services.AddScoped<GetArticleForEditingHandler>();
        services.AddScoped<CreateArticleHandler>();
        services.AddScoped<CreateArticleBlockHandler>();
        services.AddScoped<UpdateArticleBlockHandler>();
        services.AddScoped<DeleteArticleBlockHandler>();
        services.AddScoped<DeleteArticleHandler>();
        services.AddScoped<ReorderArticlesHandler>();
        services.AddScoped<GetLearningPathForManagementHandler>();
        services.AddScoped<UpdateLearningPathHandler>();
        services.AddScoped<UpdateLearningZoneHandler>();
        services.AddScoped<UpdateLearningStepHandler>();
        services.AddScoped<CreateLearningZoneHandler>();
        services.AddScoped<DeleteLearningZoneHandler>();
        services.AddScoped<ReorderLearningZonesHandler>();
        services.AddScoped<CreateLearningStepHandler>();
        services.AddScoped<DeleteLearningStepHandler>();
        services.AddScoped<ReorderLearningStepsHandler>();
        services.AddScoped<ReorderArticleBlocksHandler>();
        services.AddScoped<PublishArticleHandler>();
        services.AddScoped<UnpublishArticleHandler>();
        services.AddScoped<UpdateArticleHandler>();

        return services;
    }
}

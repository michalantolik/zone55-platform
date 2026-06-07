using BlogPlatform.Cms.Seeding.Blocks;
using Umbraco.Cms.Core.Composing;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeederComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<BlogContentSeederOptions>(
            builder.Config.GetSection("BlogContentSeeder"));

        builder.Services.Configure<BlogContentSeedOperationsOptions>(
            builder.Config.GetSection("BlogContentSeedOperations"));

        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, TextSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, HeadingSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, CodeSnippetSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, MermaidDiagramSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, PlantUmlDiagramSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, CalloutSeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, SummarySeedBlockSerializationStrategy>();
        builder.Services.AddSingleton<ISeedBlockSerializationStrategy, TableSeedBlockSerializationStrategy>();

        builder.Services.AddSingleton<BlogSeedBlockSerializationService>();

        builder.Services.AddTransient<BlogContentSeeder>();
        builder.Services.AddHostedService<BlogContentSeederHostedService>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace BlogPlatform.Cms.Seeding;

public sealed class BlogContentSeederComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<BlogContentSeederOptions>(
            builder.Config.GetSection("BlogContentSeeder"));

        builder.Services.AddTransient<BlogContentSeeder>();
        builder.Services.AddHostedService<BlogContentSeederHostedService>();
    }
}

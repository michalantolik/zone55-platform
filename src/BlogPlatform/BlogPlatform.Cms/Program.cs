using BlogPlatform.Cms.Infrastructure.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var hmacKey = builder.Configuration["Umbraco:CMS:Imaging:HMACSecretKey"];

if (string.IsNullOrWhiteSpace(hmacKey))
{
    throw new InvalidOperationException(
        "Umbraco CMS Imaging HMACSecretKey is missing.");
}

if (hmacKey.StartsWith("SET_WITH", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Umbraco CMS Imaging HMACSecretKey uses a placeholder value.");
}

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await SqlServerDatabaseInitializer.EnsureDatabaseCreatedAsync(app.Configuration);
}

await app.BootUmbracoAsync();

app.UseHttpsRedirection();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();

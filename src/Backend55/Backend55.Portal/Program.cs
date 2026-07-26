using Backend55.Portal.Components;
using Backend55.Portal.Services;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCulture);
    options.SupportedCultures = SupportedCultures.All.ToList();
    options.SupportedUICultures = SupportedCultures.All.ToList();
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<DevelopmentUserState>();
builder.Services.AddScoped<LearningNavigationState>();
builder.Services.AddHttpClient<LearnKitApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7355/");
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAntiforgery();
app.MapGet("/culture/set", (string culture, string? redirectUri, HttpContext context) =>
{
    var selected = SupportedCultures.Contains(culture) ? culture : SupportedCultures.DefaultCulture;
    context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selected)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps });
    return Results.LocalRedirect(IsLocal(redirectUri) ? redirectUri! : "/");
});
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

static bool IsLocal(string? value) => !string.IsNullOrWhiteSpace(value) && value.StartsWith('/') && !value.StartsWith("//") && !value.StartsWith("/\\");

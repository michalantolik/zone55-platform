using Microsoft.Playwright;
using NUnit.Framework;

namespace BlogPlatform.PortfolioScreenshots;

[TestFixture]
public sealed class PortfolioScreenshotTests
{
    private const string DefaultAppUrl = "https://zone55.dev";

    private static readonly string OutputDirectory =
        Path.Combine(TestContext.CurrentContext.WorkDirectory, "portfolio-screenshots");

    [Test]
    public async Task CapturePortfolioScreenshots()
    {
        Directory.CreateDirectory(OutputDirectory);

        var appUrl = GetAppUrl();

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await CreatePageAsync(browser, 1440, 1200);

        await CaptureHomeMapAsync(1, page, appUrl);
        await CaptureZoneStepPageAsync(page, appUrl, 2, "architecture-data", "design-patterns");
        await CaptureArticlePageAsync(page, appUrl, 3, "architecture-data", "design-patterns", "strategy-pattern");
        await CaptureZoneStepPageAsync(page, appUrl, 4, "cloud-advanced", "infrastructure-as-code");
        await CaptureArticlePageAsync(page, appUrl, 5, "architecture-data", "design-patterns", "main-terraform-commands");
    }

    private static async Task<IPage> CreatePageAsync(IBrowser browser, int width, int height)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = width,
                Height = height
            },
            DeviceScaleFactor = 1
        });

        return await context.NewPageAsync();
    }

    private static async Task CaptureHomeMapAsync(int number, IPage page, string appUrl)
    {
        await page.GotoAsync(appUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(OutputDirectory, $"{number.ToString("00")}-home-map-desktop.png"),
            FullPage = true
        });
    }

    private static async Task CaptureZoneStepPageAsync(IPage page, string appUrl, int number, string zone, string step)
    {
        await page.GotoAsync($"{appUrl}/{zone}/{step}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(OutputDirectory, $"{number.ToString("00")}-{zone}-{step}-desktop.png"),
            FullPage = true
        });
    }
    
    private static async Task CaptureArticlePageAsync(IPage page, string appUrl, int number, string zone, string step, string article)
    {
        await page.GotoAsync($"{appUrl}/{zone}/{step}/articles/{article}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(OutputDirectory, $"{number.ToString("00")}-{zone}-{step}-{article}-desktop.png"),
            FullPage = true
        });
    }

    private static string GetAppUrl()
    {
        var appUrl = Environment.GetEnvironmentVariable("APP_URL");

        if (string.IsNullOrWhiteSpace(appUrl))
        {
            return DefaultAppUrl;
        }

        return appUrl.TrimEnd('/');
    }
}

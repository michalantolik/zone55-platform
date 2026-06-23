using System.Globalization;
using Microsoft.Playwright;
using NUnit.Framework;

namespace BlogPlatform.PortfolioScreenshots;

[TestFixture]
public sealed class PortfolioScreenshotTests
{
    private const string DefaultAppUrl = "https://zone55.dev";
    private const int DefaultViewportWidth = 1920;
    private const int DefaultViewportHeight = 1080;
    private const float DefaultDeviceScaleFactor = 1;

    private static readonly string OutputDirectory =
        Path.Combine(TestContext.CurrentContext.WorkDirectory, "portfolio-screenshots");

    [Test]
    public async Task CapturePortfolioScreenshots()
    {
        Directory.CreateDirectory(OutputDirectory);

        var appUrl = GetAppUrl();
        var viewport = GetViewportSettings();

        TestContext.Out.WriteLine($"Portfolio screenshots URL: {appUrl}");
        TestContext.Out.WriteLine(
            $"Portfolio screenshots viewport: {viewport.Width}x{viewport.Height}, device scale factor: {viewport.DeviceScaleFactor}");
        TestContext.Out.WriteLine($"Portfolio screenshots output: {OutputDirectory}");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await CreatePageAsync(browser, viewport);

        await CaptureHomeMapAsync(1, page, appUrl);
        await CaptureZoneStepPageAsync(page, appUrl, 2, "architecture-data", "design-patterns");
        await CaptureArticlePageAsync(page, appUrl, 3, "architecture-data", "design-patterns", "strategy-pattern");
        await CaptureZoneStepPageAsync(page, appUrl, 4, "cloud-advanced", "infrastructure-as-code");
        await CaptureArticlePageAsync(page, appUrl, 5, "architecture-data", "design-patterns", "main-terraform-commands");
    }

    private static async Task<IPage> CreatePageAsync(IBrowser browser, ScreenshotViewport viewport)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = viewport.Width,
                Height = viewport.Height
            },
            ScreenSize = new ScreenSize
            {
                Width = viewport.Width,
                Height = viewport.Height
            },
            DeviceScaleFactor = viewport.DeviceScaleFactor,
            IsMobile = false
        });

        return await context.NewPageAsync();
    }

    private static async Task CaptureHomeMapAsync(int number, IPage page, string appUrl)
    {
        await NavigateAsync(page, appUrl);

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-home-map-desktop.png");
    }

    private static async Task CaptureZoneStepPageAsync(IPage page, string appUrl, int number, string zone, string step)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}");

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-{zone}-{step}-desktop.png");
    }

    private static async Task CaptureArticlePageAsync(IPage page, string appUrl, int number, string zone, string step, string article)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}/articles/{article}");

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-{zone}-{step}-{article}-desktop.png");
    }

    private static async Task NavigateAsync(IPage page, string url)
    {
        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.EvaluateAsync("window.scrollTo(0, 0)");
    }

    private static async Task CaptureVisibleViewportAsync(IPage page, string fileName)
    {
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(OutputDirectory, fileName),
            FullPage = false
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

    private static ScreenshotViewport GetViewportSettings()
    {
        return new ScreenshotViewport(
            Width: GetPositiveIntEnvironmentVariable("SCREENSHOT_VIEWPORT_WIDTH", DefaultViewportWidth),
            Height: GetPositiveIntEnvironmentVariable("SCREENSHOT_VIEWPORT_HEIGHT", DefaultViewportHeight),
            DeviceScaleFactor: GetPositiveFloatEnvironmentVariable("SCREENSHOT_DEVICE_SCALE_FACTOR", DefaultDeviceScaleFactor));
    }

    private static int GetPositiveIntEnvironmentVariable(string name, int fallbackValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);

        if (int.TryParse(rawValue, out var parsedValue) && parsedValue > 0)
        {
            return parsedValue;
        }

        return fallbackValue;
    }

    private static float GetPositiveFloatEnvironmentVariable(string name, float fallbackValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);

        if (float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue) && parsedValue > 0)
        {
            return parsedValue;
        }

        return fallbackValue;
    }

    private sealed record ScreenshotViewport(int Width, int Height, float DeviceScaleFactor);
}

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
    private const int DefaultEdgeCropWidth = 17;

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
        var edgeCropWidth = GetEdgeCropWidth();

        TestContext.Out.WriteLine($"Portfolio screenshots output: {OutputDirectory}");
        TestContext.Out.WriteLine($"Portfolio screenshots edge crop: {edgeCropWidth}px on left and right");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await CreatePageAsync(browser, viewport);

        await CaptureHomeMapAsync(1, page, appUrl, edgeCropWidth);
        await CaptureZoneStepPageAsync(page, appUrl, edgeCropWidth, 2, "architecture-data", "design-patterns");
        await CaptureArticlePageAsync(page, appUrl, edgeCropWidth, 3, "architecture-data", "design-patterns", "strategy-pattern");
        await CaptureZoneStepPageAsync(page, appUrl, edgeCropWidth, 4, "cloud-advanced", "infrastructure-as-code");
        await CaptureArticlePageAsync(page, appUrl, edgeCropWidth, 5, "architecture-data", "design-patterns", "main-terraform-commands");
    }

    private static async Task<IPage> CreatePageAsync(IBrowser browser, ScreenshotViewport viewport)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = viewport.Width,
                Height = viewportHeight
            },
            ScreenSize = new ScreenSize
            {
                Width = viewport.Width,
                Height = viewportHeight
            },
            DeviceScaleFactor = viewport.DeviceScaleFactor,
            IsMobile = false
        });

        return await context.NewPageAsync();
    }

    private static async Task CaptureHomeMapAsync(int number, IPage page, string appUrl, int edgeCropWidth)
    {
        await NavigateAsync(page, appUrl);

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-home-map-desktop.png", edgeCropWidth);
    }

    private static async Task CaptureZoneStepPageAsync(IPage page, string appUrl, int edgeCropWidth, int number, string zone, string step)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}");

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-{zone}-{step}-desktop.png", edgeCropWidth);
    }

    private static async Task CaptureArticlePageAsync(IPage page, string appUrl, int edgeCropWidth, int number, string zone, string step, string article)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}/articles/{article}");

        await CaptureVisibleViewportAsync(page, $"{number.ToString("00")}-{zone}-{step}-{article}-desktop.png", edgeCropWidth);
    }

    private static async Task NavigateAsync(IPage page, string url)
    {
        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.EvaluateAsync("window.scrollTo(0, 0)");
    }

    private static async Task CaptureVisibleViewportAsync(IPage page, string fileName, int edgeCropWidth)
    {
        var viewport = page.ViewportSize
            ?? throw new InvalidOperationException("Viewport size is not available. Configure the browser context viewport before taking screenshots.");

        var clip = CreateBalancedEdgeCrop(viewport.Width, viewport.Height, edgeCropWidth);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(OutputDirectory, fileName),
            FullPage = false,
            Clip = clip
        });
    }

    private static Clip? CreateBalancedEdgeCrop(int viewportWidth, int viewportHeight, int edgeCropWidth)
    {
        if (edgeCropWidth <= 0)
        {
            return null;
        }

        var croppedWidth = viewportWidth - (edgeCropWidth * 2);

        if (croppedWidth <= 0)
        {
            throw new InvalidOperationException(
                $"SCREENSHOT_EDGE_CROP_WIDTH={edgeCropWidth} is too large for viewport width {viewportWidth}.");
        }

        return new Clip
        {
            X = edgeCropWidth,
            Y = 0,
            Width = croppedWidth,
            Height = viewportHeight
        };
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

    private static int GetEdgeCropWidth()
    {
        return GetNonNegativeIntEnvironmentVariable("SCREENSHOT_EDGE_CROP_WIDTH", DefaultEdgeCropWidth);
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

    private static int GetNonNegativeIntEnvironmentVariable(string name, int fallbackValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);

        if (int.TryParse(rawValue, out var parsedValue) && parsedValue >= 0)
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

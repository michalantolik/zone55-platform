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

    private static readonly IReadOnlyDictionary<string, string[]> StepsByZone =
        new Dictionary<string, string[]>
        {
            ["foundation"] =
            [
                "csharp-dotnet",
                "basic-syntax",
                "types-operators",
                "functions-classes",
                "console-apps",
                "modern-csharp-dotnet"
            ],
            ["web-app-development"] =
            [
                "aspnet-core-ecosystem",
                "aspnet-core-mvc",
                "aspnet-core-web-api",
                "linq-collections",
                "dependency-injection",
                "desktop-apps"
            ],
            ["architecture-data"] =
            [
                "sql-fundamentals",
                "entity-framework-core",
                "async-programming",
                "testing",
                "clean-architecture",
                "design-patterns"
            ],
            ["cloud-advanced"] =
            [
                "microservices",
                "azure-integration",
                "cicd-pipelines",
                "performance-tuning",
                "advanced-csharp",
                "infrastructure-as-code"
            ]
        };

    private static readonly IReadOnlyDictionary<(string Zone, string Step), string[]> ArticlesByZoneStep =
        new Dictionary<(string Zone, string Step), string[]>
        {
            [("architecture-data", "design-patterns")] =
            [
                "strategy-pattern"
            ],
            [("cloud-advanced", "azure-integration")] =
            [
                "what-is-cloud-computing",
                "what-is-microsoft-azure",
                "what-is-shared-responsibility-model",
                "what-are-cloud-service-models",
                "what-are-the-cloud-deployment-models",
                "what-is-azure-arc",
                "what-is-azure-vmware-solution",
                "what-are-capex-and-opex"
            ],
            [("cloud-advanced", "cicd-pipelines")] =
            [
                "what-is-continuous-integration",
                "what-is-continuous-deployment",
                "what-is-github-actions",
                "what-is-github-actions-workflow-file",
                "how-to-create-github-actions-workflow-file",
                "what-are-the-components-of-github-actions-workflow-file",
                "typical-github-actions-pipeline-flow",
                "github-actions-pipeline-for-dotnet-app",
                "deploy-dotnet-app-to-azure-app-service-using-github-actions"
            ],
            [("cloud-advanced", "infrastructure-as-code")] =
            [
                "what-is-infrastructure-as-code",
                "core-concepts-of-infrastructure-as-code",
                "what-is-terraform",
                "terraform-versions",
                "terraform-core-components",
                "terraform-workflow",
                "installing-terraform",
                "terraform-cli-basics",
                "main-terraform-commands",
                "terraform-configuration-files",
                "vscode-extensions-for-terraform",
                "terraform-objects-and-blocks"
            ],
            [("foundation", "basic-syntax")] =
            [
                "a-simple-csharp-program"
            ],
            [("foundation", "csharp-dotnet")] =
            [
                "components-of-the-dotnet-framework",
                "what-are-cloud-deployment-models"
            ],
            [("web-app-development", "aspnet-core-ecosystem")] =
            [
                "what-is-asp-dotnet-core",
                "what-you-can-build-with-asp-dotnet-core"
            ]
        };

    [Test]
    [Ignore("Temporarily disabled")]
    public async Task CapturePortfolioScreenshots()
    {
        Directory.CreateDirectory(OutputDirectory);

        var appUrl = GetAppUrl();
        var viewport = GetViewportSettings();
        var edgeCropWidth = GetEdgeCropWidth();

        TestContext.Out.WriteLine($"Portfolio screenshots URL: {appUrl}");
        TestContext.Out.WriteLine(
            $"Portfolio screenshots viewport: {viewport.Width}x{viewport.Height}, device scale factor: {viewport.DeviceScaleFactor}");
        TestContext.Out.WriteLine($"Portfolio screenshots output: {OutputDirectory}");
        TestContext.Out.WriteLine($"Portfolio screenshots edge crop: {edgeCropWidth}px on left and right");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await CreatePageAsync(browser, viewport);

        var screenshotNumber = 1;

        await CaptureHomeMapAsync(screenshotNumber++, page, appUrl, edgeCropWidth);

        foreach (var zone in GetZones())
        {
            foreach (var step in GetSteps(zone))
            {
                await CaptureZoneStepPageAsync(page, appUrl, edgeCropWidth, screenshotNumber++, zone, step);

                foreach (var article in GetArticles(zone, step))
                {
                    await CaptureArticlePageAsync(page, appUrl, edgeCropWidth, screenshotNumber++, zone, step, article);
                }
            }
        }
    }

    [Test]
    public async Task ValidateOneScreenLandingPageAcrossSetups()
    {
        var appUrl = GetAppUrl();
        var outputDirectory = Path.Combine(OutputDirectory, "OneScreenLandingPage");
        Directory.CreateDirectory(outputDirectory);

        TestContext.Out.WriteLine($"One-screen landing page URL: {appUrl}");
        TestContext.Out.WriteLine($"One-screen landing page output: {outputDirectory}");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var failures = new List<string>();
        var screenshotNumber = 1;

        foreach (var setup in GetLandingPageUiValidationSetups())
        {
            var page = await CreatePageAsync(browser, setup.Viewport);

            try
            {
                await NavigateAsync(page, appUrl);

                var screenshotFileName = CreateOneScreenLandingPageScreenshotFileName(screenshotNumber++, setup);
                await CaptureVisibleViewportAsync(page, Path.Combine("OneScreenLandingPage", screenshotFileName), edgeCropWidth: 0);

                var documentHeight = await GetDocumentHeightAsync(page);
                var overflowHeight = documentHeight - setup.Viewport.Height;

                TestContext.Out.WriteLine(
                    $"{setup.DisplayName}: viewport {setup.Viewport.Width}x{setup.Viewport.Height}, " +
                    $"scale {setup.WindowsScalePercent}%, document height {documentHeight}px, overflow {Math.Max(0, overflowHeight)}px");

                if (overflowHeight > setup.AllowedVerticalOverflowPx)
                {
                    failures.Add(
                        $"{setup.DisplayName}: landing page is {overflowHeight}px too tall for " +
                        $"{setup.Viewport.Width}x{setup.Viewport.Height}. Shorten it by at least " +
                        $"{overflowHeight - setup.AllowedVerticalOverflowPx}px to fit one screen.");
                }
            }
            finally
            {
                await page.Context.CloseAsync();
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail("One-screen landing page validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }
    }

    private static IReadOnlyCollection<LandingPageUiValidationSetup> GetLandingPageUiValidationSetups() =>
    [
        new(
            DisplayName: "24 inch Full HD",
            PhysicalMonitor: "24_FHD",
            Resolution: "1920x1080",
            WindowsScalePercent: 100,
            Viewport: new ScreenshotViewport(1920, 940, 1)),

        new(
            DisplayName: "24 inch Full HD at 125 percent scale",
            PhysicalMonitor: "24_FHD",
            Resolution: "1920x1080",
            WindowsScalePercent: 125,
            Viewport: new ScreenshotViewport(1536, 752, 1.25f)),

        new(
            DisplayName: "27 inch QHD",
            PhysicalMonitor: "27_QHD",
            Resolution: "2560x1440",
            WindowsScalePercent: 100,
            Viewport: new ScreenshotViewport(2560, 1300, 1)),

        new(
            DisplayName: "27 inch QHD at 125 percent scale",
            PhysicalMonitor: "27_QHD",
            Resolution: "2560x1440",
            WindowsScalePercent: 125,
            Viewport: new ScreenshotViewport(2048, 1040, 1.25f)),

        new(
            DisplayName: "27 or 32 inch 4K at 150 percent scale",
            PhysicalMonitor: "27_32_4K",
            Resolution: "3840x2160",
            WindowsScalePercent: 150,
            Viewport: new ScreenshotViewport(2560, 1380, 1.5f)),

        new(
            DisplayName: "27 or 32 inch 4K at 175 percent scale",
            PhysicalMonitor: "27_32_4K",
            Resolution: "3840x2160",
            WindowsScalePercent: 175,
            Viewport: new ScreenshotViewport(2194, 1180, 1.75f)),

        new(
            DisplayName: "14 inch business laptop at 125 percent scale",
            PhysicalMonitor: "14_Laptop",
            Resolution: "1920x1200",
            WindowsScalePercent: 125,
            Viewport: new ScreenshotViewport(1536, 900, 1.25f)),

        new(
            DisplayName: "15 inch laptop at 125 percent scale",
            PhysicalMonitor: "15_Laptop",
            Resolution: "1920x1080",
            WindowsScalePercent: 125,
            Viewport: new ScreenshotViewport(1536, 752, 1.25f)),

        new(
            DisplayName: "Older laptop",
            PhysicalMonitor: "Older_Laptop",
            Resolution: "1366x768",
            WindowsScalePercent: 100,
            Viewport: new ScreenshotViewport(1366, 650, 1))
    ];

    private static string CreateOneScreenLandingPageScreenshotFileName(
        int number,
        LandingPageUiValidationSetup setup)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{number:00}_{setup.PhysicalMonitor}_{setup.Resolution}_scale{setup.WindowsScalePercent}_viewport{setup.Viewport.Width}x{setup.Viewport.Height}.png");
    }

    private static async Task<int> GetDocumentHeightAsync(IPage page)
    {
        return await page.EvaluateAsync<int>(
            "() => Math.ceil(Math.max(document.documentElement.scrollHeight, document.body.scrollHeight))");
    }

    private static string[] GetZones()
    {
        return StepsByZone.Keys.ToArray();
    }

    private static string[] GetSteps()
    {
        return StepsByZone
            .Values
            .SelectMany(steps => steps)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] GetSteps(string zone)
    {
        return StepsByZone.TryGetValue(zone, out var steps)
            ? steps
            : [];
    }

    private static string[] GetArticles(string zone, string step)
    {
        return ArticlesByZoneStep.TryGetValue((zone, step), out var articles)
            ? articles
            : [];
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

    private static async Task CaptureHomeMapAsync(int number, IPage page, string appUrl, int edgeCropWidth)
    {
        await NavigateAsync(page, appUrl);

        await CaptureVisibleViewportAsync(page, $"{number:00}-home-map-desktop.png", edgeCropWidth);
    }

    private static async Task CaptureZoneStepPageAsync(
        IPage page,
        string appUrl,
        int edgeCropWidth,
        int number,
        string zone,
        string step)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}");

        await CaptureVisibleViewportAsync(page, $"{number:00}-{zone}-{step}-desktop.png", edgeCropWidth);
    }

    private static async Task CaptureArticlePageAsync(
        IPage page,
        string appUrl,
        int edgeCropWidth,
        int number,
        string zone,
        string step,
        string article)
    {
        await NavigateAsync(page, $"{appUrl}/{zone}/{step}/articles/{article}");

        await CaptureVisibleViewportAsync(page, $"{number:00}-{zone}-{step}-{article}-desktop.png", edgeCropWidth);
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
            ?? throw new InvalidOperationException("Viewport size is not available.");

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

        var croppedWidth = viewportWidth - edgeCropWidth * 2;

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

        return string.IsNullOrWhiteSpace(appUrl)
            ? DefaultAppUrl
            : appUrl.TrimEnd('/');
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

        return int.TryParse(rawValue, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : fallbackValue;
    }

    private static int GetNonNegativeIntEnvironmentVariable(string name, int fallbackValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);

        return int.TryParse(rawValue, out var parsedValue) && parsedValue >= 0
            ? parsedValue
            : fallbackValue;
    }

    private static float GetPositiveFloatEnvironmentVariable(string name, float fallbackValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);

        return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue)
               && parsedValue > 0
            ? parsedValue
            : fallbackValue;
    }

    private sealed record ScreenshotViewport(int Width, int Height, float DeviceScaleFactor);

    private sealed record LandingPageUiValidationSetup(
        string DisplayName,
        string PhysicalMonitor,
        string Resolution,
        int WindowsScalePercent,
        ScreenshotViewport Viewport,
        int AllowedVerticalOverflowPx = 0);
}
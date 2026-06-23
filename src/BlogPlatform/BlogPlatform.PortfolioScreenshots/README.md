# Portfolio screenshots

This standalone Playwright/NUnit project captures portfolio screenshots of the deployed Zone55 application.

The screenshots are intentionally viewport-only. This means the generated image contains only the part of the page that would be visible on a maximized browser window for the configured monitor-like viewport. Long articles are not captured as one very tall full-page screenshot.

## Default viewport

The default viewport is:

```text
1920 x 1080
```

This is a practical desktop reference resolution for portfolio screenshots because it represents a common full-HD monitor size and keeps screenshots easy to review, crop, and reuse.

## Configuration

You can override the target viewport manually with environment variables:

```powershell
$env:APP_URL = "https://zone55.dev"
$env:SCREENSHOT_VIEWPORT_WIDTH = "1920"
$env:SCREENSHOT_VIEWPORT_HEIGHT = "1080"
$env:SCREENSHOT_DEVICE_SCALE_FACTOR = "1"
dotnet test .\src\BlogPlatform\BlogPlatform.PortfolioScreenshots\BlogPlatform.PortfolioScreenshots.csproj
```

Alternative useful viewport presets:

```text
1920 x 1080  Full-HD desktop reference
1440 x 900   Smaller laptop / compact portfolio preview
2560 x 1440  Large QHD monitor reference
```

The test output prints the URL, viewport size, device scale factor, and output folder used for the generated screenshots.

## Scrollbar / edge crop

Screenshots crop a small balanced stripe from both horizontal edges by default:

```text
17 px from the left
17 px from the right
```

This removes the browser scrollbar area from the right edge and removes the same width from the left edge, so the page content stays visually centered.

You can adjust or disable this crop:

```powershell
$env:SCREENSHOT_EDGE_CROP_WIDTH = "17" # default
$env:SCREENSHOT_EDGE_CROP_WIDTH = "20" # stronger crop
$env:SCREENSHOT_EDGE_CROP_WIDTH = "0"  # no crop
```

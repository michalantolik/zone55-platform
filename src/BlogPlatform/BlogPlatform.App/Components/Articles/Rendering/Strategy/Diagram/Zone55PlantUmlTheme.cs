using BlogPlatform.App.Components.Articles.Rendering.Context;

namespace BlogPlatform.App.Components.Articles.Rendering.Strategy.Diagram;

internal static class Zone55PlantUmlTheme
{
    public static readonly HashSet<string> ProtectedBlockNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "rectangle",
        "class",
        "component",
        "package",
        "node",
        "folder",
        "frame",
        "database",
        "cloud",
        "artifact",
        "storage",
        "queue",
        "file",
        "card",
        "object",
        "usecase",
        "actor",
        "entity",
        "boundary",
        "control",
        "interface",
        "collections",
        "note"
    };

    public static readonly HashSet<string> ProtectedSingleParams = new(StringComparer.OrdinalIgnoreCase)
    {
        "backgroundColor",
        "shadowing",
        "defaultFontName",
        "defaultFontColor",
        "defaultTextAlignment",
        "ArrowColor",
        "ArrowFontColor",
        "ArrowThickness",
        "TitleFontColor",
        "TitleFontSize",
        "RoundCorner",
        "Padding",
        "ParticipantPadding",
        "BoxPadding",
        "SequenceLifeLineBorderColor",
        "SequenceLifeLineBackgroundColor",
        "ParticipantBackgroundColor",
        "ParticipantBorderColor",
        "ParticipantFontColor",
        "ActorBackgroundColor",
        "ActorBorderColor",
        "ActorFontColor",
        "BoundaryBackgroundColor",
        "BoundaryBorderColor",
        "BoundaryFontColor",
        "ControlBackgroundColor",
        "ControlBorderColor",
        "ControlFontColor",
        "EntityBackgroundColor",
        "EntityBorderColor",
        "EntityFontColor",
        "DatabaseBackgroundColor",
        "DatabaseBorderColor",
        "DatabaseFontColor",
        "CollectionsBackgroundColor",
        "CollectionsBorderColor",
        "CollectionsFontColor",
        "SequenceGroupBackgroundColor",
        "SequenceGroupBorderColor",
        "SequenceGroupFontColor",
        "SequenceBoxBackgroundColor",
        "SequenceBoxBorderColor",
        "SequenceBoxFontColor",
        "SequenceDividerBackgroundColor",
        "SequenceDividerBorderColor",
        "SequenceDividerFontColor"
    };

    public static string CreateSkinParams(PlantUmlThemeValues theme)
    {
        var lineColor = NormalizePlantUmlColor(theme.Line, "#8FA9C8");
        var nodeTextColor = NormalizePlantUmlColor(theme.NodeText, "#E5F0FF");
        var nodeBorderColor = NormalizePlantUmlColor(theme.NodeBorder, "#5F7898");
        var nodeBgColor = NormalizePlantUmlColor(theme.NodeBg, "#1B2B43");
        var fontFamily = NormalizePlantUmlFontFamily(theme.FontFamily);

        var surfaceColor = DarkenHexColor(nodeBgColor, "#121D2F", 0.58);
        var groupBgColor = DarkenHexColor(nodeBgColor, "#172640", 0.35);
        var accentBgColor = LightenHexColor(nodeBgColor, "#233A5E", 0.08);
        var mutedBgColor = DarkenHexColor(nodeBgColor, "#111827", 0.18);
        var borderColor = LightenHexColor(nodeBorderColor, "#7EA0CE", 0.08);
        var softLineColor = LightenHexColor(lineColor, "#A9B8D6", 0.08);

        return $$$"""
skinparam backgroundColor transparent
skinparam shadowing false
skinparam defaultFontName {{{fontFamily}}}
skinparam defaultFontColor {{{nodeTextColor}}}
skinparam defaultTextAlignment center
skinparam RoundCorner 10
skinparam ArrowColor {{{softLineColor}}}
skinparam ArrowFontColor {{{nodeTextColor}}}
skinparam ArrowThickness 1.2
skinparam TitleFontColor {{{nodeTextColor}}}
skinparam TitleFontSize 14

skinparam rectangle {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam component {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam class {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam object {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam card {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam file {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam artifact {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam node {
    BackgroundColor {{{groupBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam folder {
    BackgroundColor {{{groupBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam frame {
    BackgroundColor {{{surfaceColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam package {
    BackgroundColor {{{surfaceColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam database {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam cloud {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam storage {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam queue {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam collections {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam interface {
    BackgroundColor {{{accentBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam actor {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam usecase {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam entity {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam boundary {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam control {
    BackgroundColor {{{nodeBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}
skinparam note {
    BackgroundColor {{{mutedBgColor}}}
    BorderColor {{{borderColor}}}
    FontColor {{{nodeTextColor}}}
}

skinparam SequenceLifeLineBorderColor {{{softLineColor}}}
skinparam SequenceLifeLineBackgroundColor {{{surfaceColor}}}
skinparam ParticipantBackgroundColor {{{nodeBgColor}}}
skinparam ParticipantBorderColor {{{borderColor}}}
skinparam ParticipantFontColor {{{nodeTextColor}}}
skinparam ActorBackgroundColor {{{nodeBgColor}}}
skinparam ActorBorderColor {{{borderColor}}}
skinparam ActorFontColor {{{nodeTextColor}}}
skinparam BoundaryBackgroundColor {{{nodeBgColor}}}
skinparam BoundaryBorderColor {{{borderColor}}}
skinparam BoundaryFontColor {{{nodeTextColor}}}
skinparam ControlBackgroundColor {{{nodeBgColor}}}
skinparam ControlBorderColor {{{borderColor}}}
skinparam ControlFontColor {{{nodeTextColor}}}
skinparam EntityBackgroundColor {{{nodeBgColor}}}
skinparam EntityBorderColor {{{borderColor}}}
skinparam EntityFontColor {{{nodeTextColor}}}
skinparam DatabaseBackgroundColor {{{accentBgColor}}}
skinparam DatabaseBorderColor {{{borderColor}}}
skinparam DatabaseFontColor {{{nodeTextColor}}}
skinparam CollectionsBackgroundColor {{{accentBgColor}}}
skinparam CollectionsBorderColor {{{borderColor}}}
skinparam CollectionsFontColor {{{nodeTextColor}}}
skinparam SequenceGroupBackgroundColor {{{surfaceColor}}}
skinparam SequenceGroupBorderColor {{{borderColor}}}
skinparam SequenceGroupFontColor {{{nodeTextColor}}}
skinparam SequenceBoxBackgroundColor {{{surfaceColor}}}
skinparam SequenceBoxBorderColor {{{borderColor}}}
skinparam SequenceBoxFontColor {{{nodeTextColor}}}
skinparam SequenceDividerBackgroundColor {{{mutedBgColor}}}
skinparam SequenceDividerBorderColor {{{borderColor}}}
skinparam SequenceDividerFontColor {{{nodeTextColor}}}
""";
    }

    private static string NormalizePlantUmlColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (TryParseRgbColor(trimmed, out var hexColor))
        {
            return hexColor;
        }

        return fallback;
    }

    private static string NormalizePlantUmlFontFamily(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "SansSerif";
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "SansSerif";
    }

    private static string DarkenHexColor(string color, string fallback, double amount)
    {
        return MixHexColor(color, fallback, amount, towardsBlack: true);
    }

    private static string LightenHexColor(string color, string fallback, double amount)
    {
        return MixHexColor(color, fallback, amount, towardsBlack: false);
    }

    private static string MixHexColor(string color, string fallback, double amount, bool towardsBlack)
    {
        if (!TryParseHexColor(color, out var r, out var g, out var b))
        {
            return fallback;
        }

        amount = Math.Clamp(amount, 0, 1);
        var target = towardsBlack ? 0 : 255;

        var mixedR = (int)Math.Round(r + ((target - r) * amount));
        var mixedG = (int)Math.Round(g + ((target - g) * amount));
        var mixedB = (int)Math.Round(b + ((target - b) * amount));

        return $"#{mixedR:X2}{mixedG:X2}{mixedB:X2}";
    }

    private static bool TryParseHexColor(string value, out int r, out int g, out int b)
    {
        r = 0;
        g = 0;
        b = 0;

        if (!value.StartsWith("#", StringComparison.Ordinal))
        {
            return false;
        }

        var hex = value[1..];

        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(character => new string(character, 2)));
        }

        if (hex.Length < 6)
        {
            return false;
        }

        hex = hex[..6];

        return int.TryParse(hex[..2], System.Globalization.NumberStyles.HexNumber, null, out r) &&
            int.TryParse(hex[2..4], System.Globalization.NumberStyles.HexNumber, null, out g) &&
            int.TryParse(hex[4..6], System.Globalization.NumberStyles.HexNumber, null, out b);
    }

    private static bool TryParseRgbColor(string value, out string hexColor)
    {
        hexColor = string.Empty;

        if (!value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var start = value.IndexOf('(', StringComparison.Ordinal);
        var end = value.IndexOf(')', StringComparison.Ordinal);

        if (start < 0 || end <= start)
        {
            return false;
        }

        var parts = value[(start + 1)..end]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length < 3 ||
            !TryParseRgbChannel(parts[0], out var r) ||
            !TryParseRgbChannel(parts[1], out var g) ||
            !TryParseRgbChannel(parts[2], out var b))
        {
            return false;
        }

        hexColor = $"#{r:X2}{g:X2}{b:X2}";
        return true;
    }

    private static bool TryParseRgbChannel(string value, out int channel)
    {
        channel = 0;

        if (value.EndsWith('%'))
        {
            if (!double.TryParse(value[..^1], out var percent))
            {
                return false;
            }

            channel = (int)Math.Round(Math.Clamp(percent, 0, 100) * 2.55);
            return true;
        }

        if (!double.TryParse(value, out var number))
        {
            return false;
        }

        channel = (int)Math.Round(Math.Clamp(number, 0, 255));
        return true;
    }
}

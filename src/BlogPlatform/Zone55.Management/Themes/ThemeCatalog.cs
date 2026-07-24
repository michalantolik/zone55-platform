namespace Zone55.Management.Themes;

public static class ThemeCatalog
{
    public const string DefaultThemeKey = "light";

    public static IReadOnlyList<ThemeDefinition> All { get; } =
    [
        new("light", "Light", "Clear neutral workspace", "theme-preview--light"),
        new("dark", "Dark", "Calm low-light workspace", "theme-preview--dark"),
        new("forest", "Forest", "Deep green and graphite", "theme-preview--forest"),
        new("ocean", "Ocean", "Muted blue and slate", "theme-preview--ocean"),
        new("ember", "Ember", "Warm copper and charcoal", "theme-preview--ember")
    ];

    public static ThemeDefinition Get(string? key) =>
        All.FirstOrDefault(theme => string.Equals(theme.Key, key, StringComparison.OrdinalIgnoreCase))
        ?? All[0];
}

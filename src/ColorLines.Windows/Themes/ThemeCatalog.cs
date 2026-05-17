namespace ColorLines.Windows.Themes;

public static class ThemeCatalog
{
    public static ThemeInfo DefaultTheme { get; } = new(
        "CozyBoard",
        "Cozy Board",
        "A warm tabletop board with soft cells and round cat pieces.");

    public static IReadOnlyList<ThemeInfo> AvailableThemes { get; } = new[] { DefaultTheme };
}

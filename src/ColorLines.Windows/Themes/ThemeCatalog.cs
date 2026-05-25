namespace ColorLines.Windows.Themes;

public static class ThemeCatalog
{
    public static ThemeInfo DefaultTheme { get; } = new(
        "CozyBoard",
        "Cozy Board",
        "温馨棋盘",
        "A warm tabletop board with soft cells and round cat pieces.");

    public static ThemeInfo ThreeDCatTokens { get; } = new(
        "3DCatTokens",
        "3D Cat Tokens",
        "立体彩猫",
        "A theme package prepared for rendered 3D-style cat tokens.");

    public static IReadOnlyList<ThemeInfo> AvailableThemes { get; } = new[] { DefaultTheme, ThreeDCatTokens };

    public static ThemeInfo GetTheme(string? id)
    {
        var normalized = Normalize(id);
        return AvailableThemes.First(theme => theme.Id == normalized);
    }

    public static string Normalize(string? id)
    {
        return AvailableThemes.Any(theme => theme.Id == id) ? id! : DefaultTheme.Id;
    }

    public static ThemeInfo GetNextTheme(string? id)
    {
        var current = Normalize(id);
        var index = AvailableThemes.ToList().FindIndex(theme => theme.Id == current);
        var nextIndex = (index + 1) % AvailableThemes.Count;
        return AvailableThemes[nextIndex];
    }

    public static Uri GetResourceUri(string? id)
    {
        return new Uri($"/ColorLines.Windows;component/Themes/{Normalize(id)}.xaml", UriKind.Relative);
    }
}

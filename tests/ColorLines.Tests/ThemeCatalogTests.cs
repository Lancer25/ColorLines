using ColorLines.Windows.Themes;

namespace ColorLines.Tests;

public sealed class ThemeCatalogTests
{
    [Fact]
    public void CatalogContainsCozyBoardTheme()
    {
        var theme = ThemeCatalog.DefaultTheme;

        Assert.Equal("CozyBoard", theme.Id);
        Assert.Equal("Cozy Board", theme.DisplayName);
        Assert.Contains("warm", theme.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AvailableThemesReturnsDefaultTheme()
    {
        Assert.Contains(ThemeCatalog.AvailableThemes, theme => theme.Id == ThemeCatalog.DefaultTheme.Id);
    }
}

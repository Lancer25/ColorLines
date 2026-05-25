using System.Xml.Linq;

namespace ColorLines.Tests;

public sealed class ReleasePackagingTests
{
    [Fact]
    public void WindowsProjectDeclaresVersionAndIconForRelease()
    {
        var project = LoadWindowsProject();
        var properties = project.Descendants("PropertyGroup").Elements()
            .ToDictionary(element => element.Name.LocalName, element => element.Value);

        Assert.Equal("0.1.0", properties["Version"]);
        Assert.Equal("0.1.0.0", properties["AssemblyVersion"]);
        Assert.Equal("0.1.0.0", properties["FileVersion"]);
        Assert.Equal("Color Lines", properties["Product"]);
        Assert.Equal("Assets\\AppIcon.ico", properties["ApplicationIcon"]);

        var iconPath = Path.Combine(ProjectRoot, "src", "ColorLines.Windows", "Assets", "AppIcon.ico");
        Assert.True(File.Exists(iconPath), "Release icon file should exist.");
        Assert.True(new FileInfo(iconPath).Length > 1000, "Release icon should not be an empty placeholder.");
    }

    [Fact]
    public void ReadmeDescribesCurrentGameAndReleaseCommands()
    {
        var readme = File.ReadAllText(Path.Combine(ProjectRoot, "README.md"));

        Assert.Contains("Version 0.1.0", readme, StringComparison.Ordinal);
        Assert.Contains("playable Windows WPF prototype", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cozy Board", readme, StringComparison.Ordinal);
        Assert.Contains("3D Cat Tokens", readme, StringComparison.Ordinal);
        Assert.Contains("--self-contained true", readme, StringComparison.Ordinal);
        Assert.Contains("--self-contained false", readme, StringComparison.Ordinal);
    }

    private static XDocument LoadWindowsProject()
    {
        return XDocument.Load(Path.Combine(ProjectRoot, "src", "ColorLines.Windows", "ColorLines.Windows.csproj"));
    }

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        ".."));
}

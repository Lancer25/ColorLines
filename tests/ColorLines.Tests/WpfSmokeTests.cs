using System.Threading;
using System.Windows;
using ColorLines.Windows;
using ColorLines.Windows.Services;

namespace ColorLines.Tests;

public sealed class WpfSmokeTests
{
    [Fact]
    public void MainWindowCanBeCreated()
    {
        Exception? thrown = null;
        var thread = new Thread(() =>
        {
            try
            {
                EnsureThemeResources();
                var savePath = Path.Combine(Path.GetTempPath(), $"color-lines-window-{Guid.NewGuid():N}.json");
                var window = new MainWindow(new LocalSaveService(savePath))
                {
                    ShowInTaskbar = false,
                    WindowState = System.Windows.WindowState.Minimized
                };
                window.Show();
                window.UpdateLayout();
                window.Close();
            }
            catch (Exception exception)
            {
                thrown = exception;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(thrown);
    }

    private static void EnsureThemeResources()
    {
        var resources = Application.Current?.Resources ?? new ResourceDictionary();
        if (Application.Current is null)
        {
            _ = new Application { Resources = resources };
        }

        if (!resources.Contains("AppBackgroundBrush"))
        {
            resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(
                    "/ColorLines.Windows;component/Themes/CozyBoard.xaml",
                    UriKind.Relative)
            });
        }
    }
}

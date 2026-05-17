using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ColorLines.Windows;
using ColorLines.Windows.Services;
using ColorLines.Windows.ViewModels;

namespace ColorLines.Tests;

public sealed class WpfSmokeTests
{
    [Fact]
    public void OccupiedCellsShowPieceBody()
    {
        Exception? thrown = null;
        var thread = new Thread(() =>
        {
            try
            {
                EnsureThemeResources();
                var savePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"color-lines-window-{Guid.NewGuid():N}.json");
                var window = new MainWindow(new LocalSaveService(savePath))
                {
                    ShowInTaskbar = false,
                    WindowState = WindowState.Minimized
                };
                window.Show();
                window.UpdateLayout();

                var occupiedButton = FindVisualChildren<Button>(window)
                    .First(button => button.DataContext is CellViewModel { IsOccupied: true });
                occupiedButton.ApplyTemplate();

                var pieceBody = FindVisualChildren<Ellipse>(occupiedButton)
                    .First(ellipse => ellipse.Name == "PieceBody");
                var moveFeedbackRing = FindVisualChildren<Border>(occupiedButton)
                    .First(border => border.Name == "MoveFeedbackRing");

                Assert.Equal(1, pieceBody.Opacity);
                Assert.Equal(0, moveFeedbackRing.Opacity);
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
            _ = new Application
            {
                Resources = resources,
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
        }
        else
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }
}

using System.Collections.Concurrent;
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
    private static readonly WpfTestThread WpfThread = new();

    [Fact]
    public void CatPieceAssetIsPackagedAsWpfResource()
    {
        RunOnWpfThread(() =>
        {
            var streamInfo = Application.GetResourceStream(new Uri(
                "/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/orange.png",
                UriKind.Relative));

            Assert.NotNull(streamInfo);
            Assert.True(streamInfo.Stream.Length > 0);
        });
    }

    [Fact]
    public void OccupiedCellsShowPieceBody()
    {
        RunOnWpfThread(() =>
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

            var pieceImage = FindVisualChildren<Image>(occupiedButton)
                .First(image => image.Name == "PieceImage");
            var pieceActor = FindVisualChildren<Grid>(occupiedButton)
                .First(grid => grid.Name == "PieceActor");
            var pieceScaleActor = FindVisualChildren<Grid>(occupiedButton)
                .First(grid => grid.Name == "PieceScaleActor");
            var pieceShadow = FindVisualChildren<Ellipse>(occupiedButton)
                .First(ellipse => ellipse.Name == "PieceShadow");
            var moveFeedbackGlow = FindVisualChildren<Ellipse>(occupiedButton)
                .First(ellipse => ellipse.Name == "MoveFeedbackGlow");
            var actorTransform = Assert.IsType<TranslateTransform>(pieceActor.RenderTransform);
            var scaleTransform = Assert.IsType<ScaleTransform>(pieceScaleActor.RenderTransform);

            Assert.Equal(44, pieceImage.Width);
            Assert.Equal(44, pieceImage.Height);
            Assert.Equal(1, pieceImage.Opacity);
            Assert.Equal(1, pieceShadow.Opacity);
            Assert.Equal(0, moveFeedbackGlow.Opacity);
            Assert.Equal(0, actorTransform.Y);
            Assert.Equal(1, scaleTransform.ScaleX);
            Assert.Equal(1, scaleTransform.ScaleY);
            window.Close();
        });
    }

    [Fact]
    public void SelectingOccupiedCellDoesNotCrashAnimationTemplate()
    {
        RunOnWpfThread(() =>
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

            occupiedButton.Command.Execute(occupiedButton.DataContext);
            window.UpdateLayout();

            window.Close();
        });
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

    private static void RunOnWpfThread(Action action)
    {
        WpfThread.Invoke(action);
    }

    private sealed class WpfTestThread
    {
        private readonly BlockingCollection<Action> actions = new();

        public WpfTestThread()
        {
            var thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "ColorLines WPF smoke test thread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void Invoke(Action action)
        {
            Exception? thrown = null;
            using var completed = new ManualResetEventSlim();
            actions.Add(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    thrown = exception;
                }
                finally
                {
                    completed.Set();
                }
            });

            Assert.True(completed.Wait(TimeSpan.FromSeconds(30)), "WPF smoke test action timed out.");
            Assert.Null(thrown);
        }

        private void Run()
        {
            _ = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            foreach (var action in actions.GetConsumingEnumerable())
            {
                action();
            }
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

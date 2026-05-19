using System.Collections.Concurrent;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
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
    public void CozyBoardThemeExposesVisualUpgradeBrushes()
    {
        RunOnWpfThread(() =>
        {
            EnsureThemeResources();

            Assert.True(Application.Current!.Resources.Contains("BoardInnerBrush"));
            Assert.True(Application.Current.Resources.Contains("BoardShadowBrush"));
            Assert.True(Application.Current.Resources.Contains("PanelBorderBrush"));
            Assert.True(Application.Current.Resources.Contains("PrimaryButtonHoverBrush"));
            Assert.True(Application.Current.Resources.Contains("MenuBackdropBrush"));
            Assert.True(Application.Current.Resources.Contains("MenuPrimaryButtonBrush"));
            Assert.True(Application.Current.Resources.Contains("MenuPrimaryButtonHoverBrush"));
            Assert.True(Application.Current.Resources.Contains("MenuSecondaryButtonBrush"));
            Assert.True(Application.Current.Resources.Contains("MenuSecondaryButtonHoverBrush"));
        });
    }

    [Fact]
    public void MainWindowStartsWithShellViewModelOnMainMenu()
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

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);

            Assert.True(shell.IsMainMenuVisible);
            Assert.False(shell.IsPlayingVisible);
            Assert.NotNull(shell.Game);
            window.Close();
        });
    }

    [Fact]
    public void MainMenuShowsActionsBeforeGameplay()
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

            var mainMenuView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "MainMenuView");
            var gameplayView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "GameplayView");
            var continueButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "ContinueButton");
            var menuSettingsButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "MenuSettingsButton");
            var menuHeroBoard = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuHeroBoard");
            var menuCommandPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuCommandPanel");
            var menuStatusStrip = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "MenuStatusStrip");
            var previewImages = FindVisualChildren<Image>(menuHeroBoard)
                .Where(image => image.Name.StartsWith("MenuPreviewCat", StringComparison.Ordinal))
                .ToArray();
            var menuBackdrop = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuBackdrop");
            var menuHeroArea = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "MenuHeroArea");

            Assert.Equal(Visibility.Visible, mainMenuView.Visibility);
            Assert.Equal(Visibility.Collapsed, gameplayView.Visibility);
            Assert.Equal(Visibility.Visible, menuBackdrop.Visibility);
            Assert.True(menuHeroArea.Margin.Left >= 24);
            Assert.NotNull(continueButton.Command);
            Assert.NotNull(menuSettingsButton.Command);
            Assert.True(continueButton.Height >= 52);
            Assert.True(menuSettingsButton.Height >= 44);
            Assert.Equal("MenuPrimaryButton", continueButton.Tag);
            Assert.Equal("MenuSecondaryButton", menuSettingsButton.Tag);
            Assert.True(menuHeroBoard.Width >= 430);
            Assert.True(menuHeroBoard.Height >= 430);
            Assert.True(menuCommandPanel.Padding.Left >= 24);
            Assert.True(menuStatusStrip.Children.Count >= 3);
            Assert.True(previewImages.Length >= 4);
            window.Close();
        });
    }

    [Fact]
    public void SettingsScreenUsesExistingGameSettingsCommands()
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

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);
            shell.OpenSettingsCommand.Execute(null);
            window.UpdateLayout();

            var settingsView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "SettingsView");
            var gameplaySettingsPanel = FindVisualChildren<Border>(window)
                .FirstOrDefault(border => border.Name == "SettingsPanel");
            var settingsToggleAnimationButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsToggleAnimationButton");
            var settingsToggleSoundButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsToggleSoundButton");
            var backToMenuButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsBackToMenuButton");

            Assert.Equal(Visibility.Visible, settingsView.Visibility);
            Assert.Null(gameplaySettingsPanel);
            Assert.Same(shell.Game.ToggleAnimationCommand, settingsToggleAnimationButton.Command);
            Assert.Same(shell.Game.ToggleSoundCommand, settingsToggleSoundButton.Command);
            Assert.Same(shell.BackToMenuCommand, backToMenuButton.Command);
            window.Close();
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

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);
            shell.ContinueCommand.Execute(null);
            window.UpdateLayout();

            var statusTextRegion = FindVisualChildren<Border>(window)
                .First(border => border.Name == "StatusTextRegion");
            var mainBoardFrame = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MainBoardFrame");
            var rightRail = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "RightRail");
            var scorePanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "ScorePanel");
            var nextCatsPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "NextCatsPanel");
            var newGameButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "NewGameButton");
            var finalScoreText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "GameOverFinalScoreText");
            var bestScoreText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "GameOverBestScoreText");
            var occupiedButton = FindVisualChildren<Button>(window)
                .First(button => button.DataContext is CellViewModel { IsOccupied: true });
            occupiedButton.ApplyTemplate();

            var scoreDeltaBadge = FindVisualChildren<Border>(window)
                .First(border => border.Name == "ScoreDeltaBadge");
            var pieceImage = FindVisualChildren<Image>(occupiedButton)
                .First(image => image.Name == "PieceImage");
            var clearPulseGlow = FindVisualChildren<Border>(occupiedButton)
                .First(border => border.Name == "ClearPulseGlow");
            var pieceActor = FindVisualChildren<Grid>(occupiedButton)
                .First(grid => grid.Name == "PieceActor");
            var pieceScaleActor = FindVisualChildren<Grid>(occupiedButton)
                .First(grid => grid.Name == "PieceScaleActor");
            var pieceShadow = FindVisualChildren<Ellipse>(occupiedButton)
                .First(ellipse => ellipse.Name == "PieceShadow");
            var pieceBase = FindVisualChildren<Ellipse>(occupiedButton)
                .First(ellipse => ellipse.Name == "PieceBase");
            var moveFeedbackGlow = FindVisualChildren<Ellipse>(occupiedButton)
                .First(ellipse => ellipse.Name == "MoveFeedbackGlow");
            var movePathPulseGlow = FindVisualChildren<Border>(occupiedButton)
                .First(border => border.Name == "MovePathPulseGlow");
            var reachableTargetGlow = FindVisualChildren<Border>(occupiedButton)
                .First(border => border.Name == "ReachableTargetGlow");
            var pathPreviewGlow = FindVisualChildren<Border>(occupiedButton)
                .First(border => border.Name == "PathPreviewGlow");
            var actorTransform = Assert.IsType<TranslateTransform>(pieceActor.RenderTransform);
            var scaleTransform = Assert.IsType<ScaleTransform>(pieceScaleActor.RenderTransform);

            Assert.Equal(44, pieceImage.Width);
            Assert.Equal(44, pieceImage.Height);
            Assert.True(statusTextRegion.MinHeight >= 54);
            Assert.True(mainBoardFrame.Padding.Left >= 18);
            Assert.True(rightRail.Margin.Left >= 24);
            Assert.True(scorePanel.Padding.Left >= 16);
            Assert.True(nextCatsPanel.Padding.Left >= 16);
            Assert.NotNull(newGameButton.Command);
            Assert.Equal("Final Score: 0", finalScoreText.Text);
            Assert.StartsWith("Best Score:", bestScoreText.Text);
            Assert.Equal(0, scoreDeltaBadge.Opacity);
            Assert.Equal(0, clearPulseGlow.Opacity);
            Assert.Equal(1, pieceImage.Opacity);
            Assert.True(pieceBase.Width >= 42);
            Assert.True(pieceBase.Opacity > 0);
            Assert.True(pieceShadow.Width >= 38);
            Assert.Equal(1, pieceShadow.Opacity);
            Assert.Equal(0, moveFeedbackGlow.Opacity);
            Assert.Equal(0, movePathPulseGlow.Opacity);
            Assert.Equal(0, reachableTargetGlow.Opacity);
            Assert.Equal(0, pathPreviewGlow.Opacity);
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

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);
            shell.ContinueCommand.Execute(null);
            window.UpdateLayout();

            var occupiedButton = FindVisualChildren<Button>(window)
                .First(button => button.DataContext is CellViewModel { IsOccupied: true });

            occupiedButton.Command.Execute(occupiedButton.DataContext);
            window.UpdateLayout();

            window.Close();
        });
    }

    [Fact]
    public void ClearFeedbackGlowDoesNotHoldPersistentOpacity()
    {
        AssertGlowDoesNotHoldPersistentOpacity("ClearFeedbackGlow");
    }

    [Fact]
    public void MoveFeedbackGlowDoesNotHoldPersistentOpacity()
    {
        AssertGlowDoesNotHoldPersistentOpacity("MoveFeedbackGlow");
    }

    [Fact]
    public void SpawnFeedbackGlowDoesNotHoldPersistentOpacity()
    {
        AssertGlowDoesNotHoldPersistentOpacity("SpawnFeedbackGlow");
    }

    [Fact]
    public void RejectFeedbackGlowDoesNotHoldPersistentOpacity()
    {
        AssertGlowDoesNotHoldPersistentOpacity("RejectFeedbackGlow");
    }

    [Fact]
    public void ScoreDeltaBadgeDoesNotHoldPersistentOpacity()
    {
        AssertGlowDoesNotHoldPersistentOpacity("ScoreDeltaBadge");
    }

    [Fact]
    public void GameOverOverlayIsOutsideContentMargin()
    {
        var mainWindowPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "ColorLines.Windows",
            "MainWindow.xaml"));
        var document = XDocument.Load(mainWindowPath);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var rootGrid = document.Root?
            .Elements()
            .First(element => element.Name.LocalName == "Grid");
        var gameplayView = rootGrid?
            .Elements()
            .First(element => element.Attribute(x + "Name")?.Value == "GameplayView");
        var gameOverOverlay = rootGrid?
            .Elements()
            .First(element => element.Attribute(x + "Name")?.Value == "GameOverOverlay");

        Assert.NotNull(rootGrid);
        Assert.Null(rootGrid!.Attribute("Margin"));
        Assert.Equal("28", gameplayView?.Attribute("Margin")?.Value);
        Assert.NotNull(gameOverOverlay);
    }

    [Fact]
    public void SoundPlayerDoesNotUseWindowsSystemSounds()
    {
        var soundPlayerPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "ColorLines.Windows",
            "Services",
            "SystemSoundPlayer.cs"));
        var source = System.IO.File.ReadAllText(soundPlayerPath);

        Assert.DoesNotContain("SystemSounds.", source);
    }

    private static void AssertGlowDoesNotHoldPersistentOpacity(string glowName)
    {
        var mainWindowPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "ColorLines.Windows",
            "MainWindow.xaml"));
        var document = XDocument.Load(mainWindowPath);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var glow = document
            .Descendants()
            .Single(element => element.Attribute(x + "Name")?.Value == glowName);

        Assert.DoesNotContain(glow.Descendants(), element =>
            element.Name.LocalName == "Setter"
            && element.Attribute("Property")?.Value == "Opacity"
            && element.Attribute("Value")?.Value == "1");
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

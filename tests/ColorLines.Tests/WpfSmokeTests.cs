using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
    public void MainWindowStartsFreshAndShowsNoticeWhenSaveIsCorrupt()
    {
        RunOnWpfThread(() =>
        {
            EnsureThemeResources();
            var savePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"color-lines-corrupt-{Guid.NewGuid():N}.json");
            File.WriteAllText(savePath, "{ bad json");
            var window = new MainWindow(new LocalSaveService(savePath))
            {
                ShowInTaskbar = false,
                WindowState = WindowState.Minimized
            };

            window.Show();
            window.UpdateLayout();

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);
            var menuNotice = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "MenuNoticeText");

            Assert.True(shell.IsMainMenuVisible);
            Assert.Contains("could not be loaded", shell.MenuNoticeText, StringComparison.Ordinal);
            Assert.Equal(shell.MenuNoticeText, menuNotice.Text);
            window.Close();
            File.Delete(savePath);
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

            var shell = Assert.IsType<ShellViewModel>(window.DataContext);
            var escapeBinding = window.InputBindings.OfType<KeyBinding>()
                .Single(binding => binding.Key == Key.Escape);
            var mainMenuView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "MainMenuView");
            var gameplayView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "GameplayView");
            var settingsView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "SettingsView");
            var pauseMenuView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "PauseMenuView");
            var continueButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "ContinueButton");
            var menuNewGameButton = FindVisualChildren<Button>(window)
                .FirstOrDefault(button => button.Name == "MenuNewGameButton");
            var menuSettingsButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "MenuSettingsButton");
            var menuHeroBoard = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuHeroBoard");
            var menuCommandPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuCommandPanel");
            var menuStatusStrip = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "MenuStatusStrip");
            var menuSaveSummary = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "MenuSaveSummary");
            var previewImages = FindVisualChildren<Image>(menuHeroBoard)
                .Where(image => image.Name.StartsWith("MenuPreviewCat", StringComparison.Ordinal))
                .ToArray();
            var menuBackdrop = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MenuBackdrop");
            var menuHeroArea = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "MenuHeroArea");

            Assert.Equal(Visibility.Visible, mainMenuView.Visibility);
            Assert.Same(shell.EscapeCommand, escapeBinding.Command);
            Assert.Equal(Visibility.Hidden, gameplayView.Visibility);
            Assert.Equal(Visibility.Hidden, settingsView.Visibility);
            Assert.Equal(Visibility.Hidden, pauseMenuView.Visibility);
            AssertShellTransitionStyle(window, mainMenuView);
            AssertShellTransitionStyle(window, gameplayView);
            AssertShellTransitionStyle(window, settingsView);
            AssertShellTransitionStyle(window, pauseMenuView);
            Assert.Equal(Visibility.Visible, menuBackdrop.Visibility);
            Assert.True(menuHeroArea.Margin.Left >= 24);
            Assert.NotNull(continueButton.Command);
            Assert.True(continueButton.IsEnabled);
            Assert.NotNull(menuNewGameButton);
            Assert.NotNull(menuSettingsButton.Command);
            Assert.True(continueButton.Height >= 52);
            Assert.True(menuNewGameButton.Height >= 44);
            Assert.True(menuSettingsButton.Height >= 44);
            Assert.Equal("MenuPrimaryButton", continueButton.Tag);
            Assert.Same(shell.ContinueCommand, continueButton.Command);
            Assert.Equal("MenuSecondaryButton", menuNewGameButton.Tag);
            Assert.Equal("MenuSecondaryButton", menuSettingsButton.Tag);
            Assert.Same(shell.NewGameCommand, menuNewGameButton.Command);
            Assert.True(menuHeroBoard.Width >= 430);
            Assert.True(menuHeroBoard.Height >= 430);
            Assert.True(menuCommandPanel.Padding.Left >= 24);
            Assert.True(menuStatusStrip.Children.Count >= 5);
            Assert.Equal(shell.SaveSummaryText, menuSaveSummary.Text);
            Assert.True(previewImages.Length >= 4);
            window.Close();
        });
    }

    [Fact]
    public void MainMenuDisablesContinueAfterGameOver()
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
            shell.OpenPauseMenuCommand.Execute(null);
            shell.EndGameCommand.Execute(null);
            shell.ConfirmEndGameCommand.Execute(null);
            shell.BackToMenuCommand.Execute(null);
            window.UpdateLayout();

            var continueButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "ContinueButton");
            var menuSaveSummary = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "MenuSaveSummary");

            Assert.False(continueButton.IsEnabled);
            Assert.True(continueButton.Opacity <= 0.65);
            Assert.Equal(shell.SaveSummaryText, menuSaveSummary.Text);
            Assert.Contains("Start a new game", menuSaveSummary.Text, StringComparison.Ordinal);
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
            var settingsShell = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "SettingsShell");
            var settingsHeader = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "SettingsHeader");
            var settingsContentPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "SettingsContentPanel");
            var settingsOptionList = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "SettingsOptionList");
            var animationSettingRow = FindVisualChildren<Border>(window)
                .First(border => border.Name == "AnimationSettingRow");
            var soundSettingRow = FindVisualChildren<Border>(window)
                .First(border => border.Name == "SoundSettingRow");
            var languageSettingRow = FindVisualChildren<Border>(window)
                .First(border => border.Name == "LanguageSettingRow");
            var difficultySettingRow = FindVisualChildren<Border>(window)
                .First(border => border.Name == "DifficultySettingRow");
            var settingsActionBar = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "SettingsActionBar");
            var gameplaySettingsPanel = FindVisualChildren<Border>(window)
                .FirstOrDefault(border => border.Name == "SettingsPanel");
            var settingsToggleAnimationButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsToggleAnimationButton");
            var settingsToggleSoundButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsToggleSoundButton");
            var themeSettingRow = FindVisualChildren<Border>(window)
                .First(border => border.Name == "ThemeSettingRow");
            var settingsThemeButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsThemeButton");
            var settingsChineseButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsChineseButton");
            var settingsHardButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsHardButton");
            var backToMenuButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsBackToMenuButton");
            var settingsNewGameButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsNewGameButton");

            Assert.Equal(Visibility.Visible, settingsView.Visibility);
            Assert.Equal(Visibility.Visible, settingsShell.Visibility);
            Assert.True(settingsShell.ColumnDefinitions[1].Width.IsStar);
            Assert.True(settingsHeader.Children.Count >= 2);
            Assert.True(settingsContentPanel.Padding.Left >= 24);
            Assert.True(settingsOptionList.Children.Count >= 2);
            Assert.True(animationSettingRow.MinHeight >= 82);
            Assert.True(soundSettingRow.MinHeight >= 82);
            Assert.True(themeSettingRow.MinHeight >= 82);
            Assert.True(languageSettingRow.MinHeight >= 82);
            Assert.True(difficultySettingRow.MinHeight >= 82);
            Assert.True(settingsActionBar.Children.Count >= 2);
            Assert.Equal("MenuSecondaryButton", settingsToggleAnimationButton.Tag);
            Assert.Equal("MenuSecondaryButton", settingsToggleSoundButton.Tag);
            Assert.Equal("MenuSecondaryButton", settingsThemeButton.Tag);
            Assert.Equal("MenuSecondaryButton", settingsChineseButton.Tag);
            Assert.Equal("MenuSecondaryButton", settingsHardButton.Tag);
            Assert.Equal("MenuPrimaryButton", settingsNewGameButton.Tag);
            Assert.Equal("MenuSecondaryButton", backToMenuButton.Tag);
            Assert.Null(gameplaySettingsPanel);
            Assert.Same(shell.Game.ToggleAnimationCommand, settingsToggleAnimationButton.Command);
            Assert.Same(shell.Game.ToggleSoundCommand, settingsToggleSoundButton.Command);
            Assert.Null(settingsThemeButton.Command);
            Assert.Same(shell.SetLanguageCommand, settingsChineseButton.Command);
            Assert.Same(shell.Game.SetDifficultyCommand, settingsHardButton.Command);
            Assert.Same(shell.NewGameCommand, settingsNewGameButton.Command);
            Assert.Same(shell.CloseSettingsCommand, backToMenuButton.Command);
            window.Close();
        });
    }

    [Fact]
    public void LanguageSwitchUpdatesVisibleMenuAndSettingsCopy()
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
            shell.SetLanguageCommand.Execute("zh");
            shell.OpenSettingsCommand.Execute(null);
            window.UpdateLayout();

            var readyText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "ReadyToPlayText");
            var scoreLabel = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "ScoreLabelText");
            var nextCatsLabel = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "NextCatsLabelText");
            var settingsChineseButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsChineseButton");
            var settingsToggleSoundButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsToggleSoundButton");
            var settingsThemeButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "SettingsThemeButton");
            var difficultySummary = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "DifficultySummaryText");

            Assert.Equal("准备开始", readyText.Text);
            Assert.Equal("分数", scoreLabel.Text);
            Assert.Equal("下批猫咪", nextCatsLabel.Text);
            Assert.Equal("关闭声音", settingsToggleSoundButton.Content);
            Assert.Equal("温馨棋盘", settingsThemeButton.Content);
            Assert.Equal("中文", settingsChineseButton.Content);
            Assert.Equal(shell.DifficultySummaryText, difficultySummary.Text);
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

            var gameplayView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "GameplayView");
            var gameplayShell = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "GameplayShell");
            var gameplayBoardArea = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "GameplayBoardArea");
            var gameplayHudPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "GameplayHudPanel");
            var gameplayStatusBanner = FindVisualChildren<Border>(window)
                .First(border => border.Name == "GameplayStatusBanner");
            var gameplayScoreBlock = FindVisualChildren<Border>(window)
                .First(border => border.Name == "GameplayScoreBlock");
            var gameplayNextCatsBlock = FindVisualChildren<Border>(window)
                .First(border => border.Name == "GameplayNextCatsBlock");
            var gameplayActionBar = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "GameplayActionBar");
            var pauseMenuView = FindVisualChildren<Grid>(window)
                .First(grid => grid.Name == "PauseMenuView");
            var pauseMenuPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "PauseMenuPanel");
            var pauseMenuActionList = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "PauseMenuActionList");
            var pauseContinueButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "PauseContinueButton");
            var pauseSaveButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "PauseSaveButton");
            var pauseSettingsButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "PauseSettingsButton");
            var pauseEndGameButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "PauseEndGameButton");
            var endGameConfirmPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "EndGameConfirmPanel");
            var endGameCancelButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "EndGameCancelButton");
            var endGameConfirmButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "EndGameConfirmButton");
            var pauseBackToMenuButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "PauseBackToMenuButton");
            var returnToMenuConfirmPanel = FindVisualChildren<Border>(window)
                .First(border => border.Name == "ReturnToMenuConfirmPanel");
            var returnToMenuCancelButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "ReturnToMenuCancelButton");
            var returnToMenuConfirmButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "ReturnToMenuConfirmButton");
            var pauseSaveStatusText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "PauseSaveStatusText");
            var mainBoardFrame = FindVisualChildren<Border>(window)
                .First(border => border.Name == "MainBoardFrame");
            var boardGrid = FindVisualChildren<UniformGrid>(window)
                .First(grid => grid.Rows == shell.Game.BoardSize && grid.Columns == shell.Game.BoardSize);
            var menuButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "GameplayMenuButton");
            var newGameButton = FindVisualChildren<Button>(window)
                .FirstOrDefault(button => button.Name == "NewGameButton");
            var settingsButton = FindVisualChildren<Button>(window)
                .FirstOrDefault(button => button.Name == "GameplaySettingsButton");
            var finalScoreText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "GameOverFinalScoreText");
            var bestScoreText = FindVisualChildren<TextBlock>(window)
                .First(textBlock => textBlock.Name == "GameOverBestScoreText");
            var gameOverDialog = FindVisualChildren<Border>(window)
                .First(border => border.Name == "GameOverDialog");
            var gameOverActionBar = FindVisualChildren<StackPanel>(window)
                .First(panel => panel.Name == "GameOverActionBar");
            var gameOverNewGameButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "GameOverNewGameButton");
            var gameOverMenuButton = FindVisualChildren<Button>(window)
                .First(button => button.Name == "GameOverMenuButton");
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
            var pathPreviewTargetGlow = FindVisualChildren<Border>(occupiedButton)
                .First(border => border.Name == "PathPreviewTargetGlow");
            var actorTransform = Assert.IsType<TranslateTransform>(pieceActor.RenderTransform);
            var scaleTransform = Assert.IsType<ScaleTransform>(pieceScaleActor.RenderTransform);

            Assert.True(pieceImage.Width > 0);
            Assert.True(pieceImage.Width <= 44);
            Assert.Equal(pieceImage.Width, pieceImage.Height);
            Assert.Equal(Visibility.Visible, gameplayView.Visibility);
            AssertShellTransitionStyle(window, gameplayView);
            AssertShellTransitionStyle(window, pauseMenuView);
            Assert.Equal(Visibility.Visible, gameplayShell.Visibility);
            Assert.True(gameplayBoardArea.ColumnDefinitions.Count >= 1);
            Assert.True(gameplayHudPanel.Padding.Left >= 18);
            Assert.True(gameplayHudPanel.MaxWidth <= 340);
            Assert.True(gameplayStatusBanner.MinHeight >= 58);
            Assert.True(gameplayScoreBlock.Padding.Left >= 16);
            Assert.True(gameplayNextCatsBlock.Padding.Left >= 16);
            Assert.Single(gameplayActionBar.Children);
            Assert.NotNull(pauseMenuView);
            Assert.Equal(Visibility.Hidden, pauseMenuView.Visibility);
            Assert.True(pauseMenuPanel.Padding.Left >= 24);
            Assert.True(pauseMenuActionList.Children.Count >= 5);
            Assert.True(mainBoardFrame.Padding.Left >= 18);
            Assert.Equal(shell.Game.BoardSize, boardGrid.Rows);
            Assert.Equal(shell.Game.BoardSize, boardGrid.Columns);
            Assert.Same(shell.OpenPauseMenuCommand, menuButton.Command);
            Assert.Null(newGameButton);
            Assert.Null(settingsButton);
            Assert.Same(shell.BackToGameCommand, pauseContinueButton.Command);
            Assert.Same(shell.SaveGameCommand, pauseSaveButton.Command);
            Assert.Same(shell.OpenSettingsCommand, pauseSettingsButton.Command);
            Assert.Same(shell.EndGameCommand, pauseEndGameButton.Command);
            Assert.Equal(Visibility.Collapsed, endGameConfirmPanel.Visibility);
            Assert.Same(endGameCancelButton, FocusManager.GetFocusedElement(endGameConfirmPanel));
            Assert.Same(shell.CancelEndGameCommand, endGameCancelButton.Command);
            Assert.Same(shell.ConfirmEndGameCommand, endGameConfirmButton.Command);
            Assert.Same(shell.RequestBackToMenuCommand, pauseBackToMenuButton.Command);
            Assert.Same(pauseBackToMenuButton, pauseMenuActionList.Children[^2]);
            Assert.Same(returnToMenuConfirmPanel, pauseMenuActionList.Children[^1]);
            Assert.Equal(Visibility.Collapsed, returnToMenuConfirmPanel.Visibility);
            Assert.Same(returnToMenuCancelButton, FocusManager.GetFocusedElement(returnToMenuConfirmPanel));
            Assert.Same(shell.CancelBackToMenuCommand, returnToMenuCancelButton.Command);
            Assert.Same(shell.ConfirmBackToMenuCommand, returnToMenuConfirmButton.Command);
            Assert.Equal("MenuSecondaryButton", menuButton.Tag);
            Assert.Equal(string.Empty, pauseSaveStatusText.Text);
            Assert.Equal("Final Score: 0", finalScoreText.Text);
            Assert.StartsWith("Best Score:", bestScoreText.Text);
            Assert.True(gameOverDialog.Padding.Left >= 24);
            Assert.True(gameOverActionBar.Children.Count >= 2);
            Assert.Same(shell.NewGameCommand, gameOverNewGameButton.Command);
            Assert.Same(shell.BackToMenuCommand, gameOverMenuButton.Command);
            Assert.Equal("MenuPrimaryButton", gameOverNewGameButton.Tag);
            Assert.Equal("MenuSecondaryButton", gameOverMenuButton.Tag);
            Assert.Equal(0, scoreDeltaBadge.Opacity);
            Assert.Equal(0, clearPulseGlow.Opacity);
            Assert.Equal(1, pieceImage.Opacity);
            Assert.True(pieceBase.Width >= 40);
            Assert.True(pieceBase.Opacity > 0);
            Assert.True(pieceShadow.Width >= 36);
            Assert.Equal(1, pieceShadow.Opacity);
            Assert.Equal(0, moveFeedbackGlow.Opacity);
            Assert.Equal(0, movePathPulseGlow.Opacity);
            Assert.Equal(0, reachableTargetGlow.Opacity);
            Assert.Equal(0, pathPreviewGlow.Opacity);
            Assert.Equal(0, pathPreviewTargetGlow.Opacity);
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
    public void TransientFeedbackLayersRespectAnimationIntensity()
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
        var feedbackLayerNames = new[]
        {
            "MovePathPulseGlow",
            "ClearFeedbackGlow",
            "ClearPulseGlow",
            "RejectFeedbackGlow",
            "SpawnFeedbackGlow",
            "MoveFeedbackGlow",
            "ScoreDeltaBadge"
        };

        foreach (var feedbackLayerName in feedbackLayerNames)
        {
            var layer = document
                .Descendants()
                .Single(element => element.Attribute(x + "Name")?.Value == feedbackLayerName);

            Assert.Contains("Game.IsFullAnimation", layer.ToString(SaveOptions.DisableFormatting));
        }

        var pieceScaleActor = document
            .Descendants()
            .Single(element => element.Attribute(x + "Name")?.Value == "PieceScaleActor");

        Assert.Contains("Game.IsFullAnimation", pieceScaleActor.ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public void PathPreviewGlowUsesFlowingAnimation()
    {
        var document = LoadMainWindowXaml();
        var x = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
        var pathPreviewGlow = document.Descendants()
            .First(element => element.Name.LocalName == "Border"
                && element.Attribute(x + "Name")?.Value == "PathPreviewGlow");

        var previewMarkup = pathPreviewGlow.ToString(SaveOptions.DisableFormatting);

        Assert.Contains("PathPreviewFlowStoryboard", previewMarkup, StringComparison.Ordinal);
        Assert.Contains("RepeatBehavior=\"Forever\"", previewMarkup, StringComparison.Ordinal);
        Assert.Contains("AutoReverse=\"True\"", previewMarkup, StringComparison.Ordinal);
    }

    [Fact]
    public void ReducedAnimationKeepsFeedbackGlowsSubtleAndScoreDeltaVisible()
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
        var feedbackLayerNames = new[]
        {
            "MovePathPulseGlow",
            "ClearFeedbackGlow",
            "ClearPulseGlow",
            "RejectFeedbackGlow",
            "SpawnFeedbackGlow",
            "MoveFeedbackGlow"
        };

        foreach (var feedbackLayerName in feedbackLayerNames)
        {
            var layer = document
                .Descendants()
                .Single(element => element.Attribute(x + "Name")?.Value == feedbackLayerName);
            var reducedTrigger = layer
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "MultiDataTrigger"
                    && element.ToString(SaveOptions.DisableFormatting).Contains("Game.IsFullAnimation")
                    && element.ToString(SaveOptions.DisableFormatting).Contains("Value=\"False\""));
            var opacitySetter = reducedTrigger
                .Descendants()
                .Single(element =>
                    element.Name.LocalName == "Setter"
                    && element.Attribute("Property")?.Value == "Opacity");

            var opacity = double.Parse(opacitySetter.Attribute("Value")!.Value, CultureInfo.InvariantCulture);
            Assert.True(opacity <= 0.08, $"{feedbackLayerName} reduced opacity should stay subtle.");
        }

        var scoreDeltaBadge = document
            .Descendants()
            .Single(element => element.Attribute(x + "Name")?.Value == "ScoreDeltaBadge");
        var reducedScoreTrigger = scoreDeltaBadge
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "MultiDataTrigger"
                && HasAnimationIntensityCondition(element, "False"));
        var reducedScoreOpacitySetter = reducedScoreTrigger
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "Setter"
                && element.Attribute("Property")?.Value == "Opacity");
        var reducedScoreOpacity = double.Parse(
            reducedScoreOpacitySetter.Attribute("Value")!.Value,
            CultureInfo.InvariantCulture);

        Assert.InRange(reducedScoreOpacity, 0.5, 1);

        var fullScoreTrigger = scoreDeltaBadge
            .Descendants()
            .Single(element =>
                element.Name.LocalName == "MultiDataTrigger"
                && HasAnimationIntensityCondition(element, "True"));

        Assert.Contains(fullScoreTrigger.Descendants(), element =>
            element.Name.LocalName == "DoubleAnimation"
            && element.Attribute("Storyboard.TargetProperty")?.Value == "Opacity");

        static bool HasAnimationIntensityCondition(XElement trigger, string value)
        {
            return trigger
                .Elements()
                .Where(element => element.Name.LocalName == "MultiDataTrigger.Conditions")
                .Elements()
                .Any(element =>
                    element.Name.LocalName == "Condition"
                    && element.Attribute("Binding")?.Value.Contains("Game.IsFullAnimation", StringComparison.Ordinal) == true
                    && element.Attribute("Value")?.Value == value);
        }
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
        var document = LoadMainWindowXaml();
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var glow = document
            .Descendants()
            .Single(element => element.Attribute(x + "Name")?.Value == glowName);

        Assert.DoesNotContain(glow.Descendants(), element =>
            element.Name.LocalName == "Setter"
            && element.Attribute("Property")?.Value == "Opacity"
            && element.Attribute("Value")?.Value == "1");
    }

    private static XDocument LoadMainWindowXaml()
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
        return XDocument.Load(mainWindowPath);
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

    private static void AssertShellTransitionStyle(Window window, Grid view)
    {
        Assert.Same(window.FindResource("ShellViewTransitionStyle"), view.Style);
        Assert.IsType<TranslateTransform>(view.RenderTransform);
    }
}

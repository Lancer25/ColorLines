# Color Lines Main Menu Shell Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a traditional main menu flow so the app starts on a menu, enters a focused gameplay screen, and moves settings out of the always-visible game HUD.

**Architecture:** Add a small WPF shell view model that wraps the existing `GameViewModel` and owns screen navigation. Keep game rules, save contracts, cat assets, and `GameViewModel` gameplay behavior unchanged. Update `MainWindow` to bind to the shell while board controls bind through `Game`.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## File Structure

- Create `src/ColorLines.Windows/ViewModels/ShellScreen.cs`: enum for `MainMenu`, `Playing`, and `Settings`.
- Create `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`: navigation commands and wrapper around `GameViewModel`.
- Modify `src/ColorLines.Windows/MainWindow.xaml.cs`: construct `ShellViewModel`, save through `ShellViewModel.Game`, and forward board hover commands through the shell.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: add menu/settings/game views and bind gameplay controls through `Game`.
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`: add shell navigation tests.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: update smoke tests for menu-first launch, playing screen, settings screen, and game-over overlay visibility.
- Modify `docs/superpowers/plans/2026-05-19-color-lines-main-menu-shell.md`: mark tasks complete as they are finished.

---

## Task 1: Add Shell Navigation View Model

**Files:**
- Create: `src/ColorLines.Windows/ViewModels/ShellScreen.cs`
- Create: `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [x] **Step 1: Write failing shell navigation tests**

Add these tests to `tests/ColorLines.Tests/GameViewModelTests.cs` before `SequenceRandomSource`:

```csharp
[Fact]
public void ShellViewModelStartsAtMainMenu()
{
    var game = GameViewModel.CreateForNewGame();
    var shell = new ShellViewModel(game);

    Assert.Equal(ShellScreen.MainMenu, shell.CurrentScreen);
    Assert.True(shell.IsMainMenuVisible);
    Assert.False(shell.IsPlayingVisible);
    Assert.False(shell.IsSettingsVisible);
    Assert.Same(game, shell.Game);
}

[Fact]
public void ContinueCommandOpensPlayingScreen()
{
    var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

    shell.ContinueCommand.Execute(null);

    Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
    Assert.False(shell.IsMainMenuVisible);
    Assert.True(shell.IsPlayingVisible);
}

[Fact]
public void SettingsCommandOpensSettingsScreenAndBackReturnsToMenu()
{
    var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

    shell.OpenSettingsCommand.Execute(null);

    Assert.Equal(ShellScreen.Settings, shell.CurrentScreen);
    Assert.True(shell.IsSettingsVisible);

    shell.BackToMenuCommand.Execute(null);

    Assert.Equal(ShellScreen.MainMenu, shell.CurrentScreen);
}

[Fact]
public void NewGameCommandResetsGameAndOpensPlayingScreen()
{
    var shell = new ShellViewModel(GameViewModel.CreateForNewGame());
    var originalGame = shell.Game;
    var occupied = originalGame.Cells.First(cell => cell.IsOccupied);
    originalGame.SelectCellCommand.Execute(occupied);

    shell.NewGameCommand.Execute(null);

    Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
    Assert.Same(originalGame, shell.Game);
    Assert.Equal(0, shell.Game.Score);
    Assert.DoesNotContain(shell.Game.Cells, cell => cell.IsSelected);
}
```

- [x] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "ShellViewModel|ShellScreen"
```

Expected: FAIL because `ShellViewModel` and `ShellScreen` do not exist.

- [x] **Step 3: Add `ShellScreen`**

Create `src/ColorLines.Windows/ViewModels/ShellScreen.cs`:

```csharp
namespace ColorLines.Windows.ViewModels;

public enum ShellScreen
{
    MainMenu,
    Playing,
    Settings
}
```

- [x] **Step 4: Add `ShellViewModel`**

Create `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ColorLines.Windows.ViewModels;

public sealed class ShellViewModel : INotifyPropertyChanged
{
    private GameViewModel game;
    private ShellScreen currentScreen;

    public ShellViewModel(GameViewModel game)
    {
        this.game = game;
        currentScreen = ShellScreen.MainMenu;
        ContinueCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        NewGameCommand = new RelayCommand(_ => StartNewGame());
        OpenSettingsCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Settings);
        BackToMenuCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.MainMenu);
        BackToGameCommand = new RelayCommand(_ => CurrentScreen = ShellScreen.Playing);
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke(this, EventArgs.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? ExitRequested;

    public GameViewModel Game
    {
        get => game;
        private set
        {
            if (!ReferenceEquals(game, value))
            {
                game = value;
                OnPropertyChanged();
            }
        }
    }

    public ShellScreen CurrentScreen
    {
        get => currentScreen;
        private set
        {
            if (currentScreen != value)
            {
                currentScreen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMainMenuVisible));
                OnPropertyChanged(nameof(IsPlayingVisible));
                OnPropertyChanged(nameof(IsSettingsVisible));
            }
        }
    }

    public bool IsMainMenuVisible => CurrentScreen == ShellScreen.MainMenu;

    public bool IsPlayingVisible => CurrentScreen == ShellScreen.Playing;

    public bool IsSettingsVisible => CurrentScreen == ShellScreen.Settings;

    public ICommand ContinueCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand BackToMenuCommand { get; }

    public ICommand BackToGameCommand { get; }

    public ICommand ExitCommand { get; }

    private void StartNewGame()
    {
        Game.NewGameCommand.Execute(null);
        CurrentScreen = ShellScreen.Playing;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

- [x] **Step 5: Run shell tests**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "ShellViewModel|ShellScreen"
```

Expected: PASS.

- [x] **Step 6: Commit**

```powershell
git add src\ColorLines.Windows\ViewModels\ShellScreen.cs src\ColorLines.Windows\ViewModels\ShellViewModel.cs tests\ColorLines.Tests\GameViewModelTests.cs
git commit -m "feat: add main menu shell navigation"
```

---

## Task 2: Bind MainWindow To ShellViewModel

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml.cs`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing WPF DataContext smoke test**

Add this test to `tests/ColorLines.Tests/WpfSmokeTests.cs` after `CozyBoardThemeExposesVisualUpgradeBrushes`:

```csharp
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
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainWindowStartsWithShellViewModelOnMainMenu
```

Expected: FAIL because `MainWindow.DataContext` is still `GameViewModel`.

- [x] **Step 3: Update `MainWindow.xaml.cs` to use shell**

Replace the `viewModel` field with:

```csharp
private readonly ShellViewModel shellViewModel;
```

In the constructor, replace:

```csharp
viewModel = GameViewModel.CreateFromSave(save);
DataContext = viewModel;
```

with:

```csharp
shellViewModel = new ShellViewModel(GameViewModel.CreateFromSave(save));
shellViewModel.ExitRequested += (_, _) => Close();
DataContext = shellViewModel;
```

In `OnClosing`, replace:

```csharp
saveService.Save(viewModel.CreateSaveData(new WindowPlacementData(Width, Height)));
```

with:

```csharp
saveService.Save(shellViewModel.Game.CreateSaveData(new WindowPlacementData(Width, Height)));
```

In `BoardCellPointerEntered`, replace the `DataContext` check with:

```csharp
if (DataContext is ShellViewModel shell && sender is FrameworkElement element)
{
    shell.Game.PreviewPathCommand.Execute(element.DataContext);
}
```

In `BoardCellPointerLeft`, replace the `DataContext` check with:

```csharp
if (DataContext is ShellViewModel shell)
{
    shell.Game.ClearPreviewPathCommand.Execute(null);
}
```

- [x] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainWindowStartsWithShellViewModelOnMainMenu
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml.cs tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: bind main window to shell view model"
```

---

## Task 3: Add Main Menu And Screen Visibility

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing smoke test for menu and hidden game screen**

Add this test to `tests/ColorLines.Tests/WpfSmokeTests.cs` after `MainWindowStartsWithShellViewModelOnMainMenu`:

```csharp
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

        Assert.Equal(Visibility.Visible, mainMenuView.Visibility);
        Assert.Equal(Visibility.Collapsed, gameplayView.Visibility);
        Assert.NotNull(continueButton.Command);
        Assert.NotNull(menuSettingsButton.Command);
        window.Close();
    });
}
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because `MainMenuView` does not exist.

- [x] **Step 3: Wrap existing gameplay grid**

In `src/ColorLines.Windows/MainWindow.xaml`, rename the existing `ContentGrid` to `GameplayView` and bind visibility:

```xml
<Grid x:Name="GameplayView"
      Margin="28"
      Visibility="{Binding IsPlayingVisible, Converter={StaticResource BooleanToVisibilityConverter}}">
```

Because the root `DataContext` is now `ShellViewModel`, update gameplay bindings:

- `ItemsControl ItemsSource="{Binding Cells}"` becomes `ItemsSource="{Binding Game.Cells}"`.
- Board cell command binding becomes:

```xml
Command="{Binding DataContext.Game.SelectCellCommand, RelativeSource={RelativeSource AncestorType=Window}}"
```

- Right rail text and panel bindings gain `Game.` prefix:
  - `StatusText`
  - `Score`
  - `HighScore`
  - `ShowScoreDelta`
  - `ScoreDeltaBadgeText`
  - `NextPieces`
  - `SelectedThemeName`
  - `AnimationIntensity`
  - `AnimationToggleText`
  - `ToggleAnimationCommand`
  - `ToggleSoundCommand`
  - `IsSoundEnabled`

Keep `NewGameButton` bound to the shell command:

```xml
Command="{Binding NewGameCommand}"
```

- [x] **Step 4: Add `MainMenuView` before `GameplayView`**

Add this grid as the first child of the root `Grid`, before `GameplayView`:

```xml
<Grid x:Name="MainMenuView"
      Margin="28"
      Visibility="{Binding IsMainMenuVisible, Converter={StaticResource BooleanToVisibilityConverter}}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="340" />
    </Grid.ColumnDefinitions>

    <StackPanel Grid.Column="0"
                VerticalAlignment="Center"
                Margin="36,0,48,0">
        <TextBlock Text="Color Lines"
                   FontSize="54"
                   FontWeight="Bold"
                   Foreground="{StaticResource TextPrimaryBrush}" />
        <TextBlock Text="Cute cats. Classic lines. One cozy board."
                   Margin="0,10,0,0"
                   FontSize="18"
                   Foreground="{StaticResource TextMutedBrush}" />
        <Border Margin="0,32,0,0"
                Background="{StaticResource BoardShadowBrush}"
                BorderBrush="{StaticResource BoardBorderBrush}"
                BorderThickness="2"
                CornerRadius="16"
                Padding="18"
                MaxWidth="360"
                HorizontalAlignment="Left">
            <UniformGrid Rows="3" Columns="3">
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68">
                    <Image Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/orange.png"
                           Width="46"
                           Height="46"
                           Stretch="Uniform"
                           RenderOptions.BitmapScalingMode="HighQuality" />
                </Border>
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68">
                    <Image Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/bluegray.png"
                           Width="46"
                           Height="46"
                           Stretch="Uniform"
                           RenderOptions.BitmapScalingMode="HighQuality" />
                </Border>
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68">
                    <Image Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/calico.png"
                           Width="46"
                           Height="46"
                           Stretch="Uniform"
                           RenderOptions.BitmapScalingMode="HighQuality" />
                </Border>
                <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="8" Margin="4" Height="68" />
            </UniformGrid>
        </Border>
    </StackPanel>

    <Border Grid.Column="1"
            VerticalAlignment="Center"
            Background="{StaticResource PanelBackgroundBrush}"
            BorderBrush="{StaticResource PanelBorderBrush}"
            BorderThickness="1"
            CornerRadius="10"
            Padding="24">
        <StackPanel>
            <TextBlock Text="{Binding Game.HighScore, StringFormat=Best Score: {0}}"
                       FontSize="18"
                       FontWeight="SemiBold"
                       Foreground="{StaticResource TextPrimaryBrush}" />
            <Button x:Name="ContinueButton"
                    Content="Continue"
                    Command="{Binding ContinueCommand}"
                    Height="44"
                    Margin="0,22,0,0"
                    Background="{StaticResource AccentBrush}"
                    Foreground="#FFFFFB"
                    BorderThickness="0"
                    FontSize="15"
                    FontWeight="SemiBold" />
            <Button x:Name="MenuNewGameButton"
                    Content="New Game"
                    Command="{Binding NewGameCommand}"
                    Height="40"
                    Margin="0,10,0,0"
                    Background="{StaticResource CellHoverBrush}"
                    Foreground="{StaticResource TextPrimaryBrush}"
                    BorderBrush="{StaticResource CellBorderBrush}" />
            <Button x:Name="MenuSettingsButton"
                    Content="Settings"
                    Command="{Binding OpenSettingsCommand}"
                    Height="40"
                    Margin="0,10,0,0"
                    Background="{StaticResource CellHoverBrush}"
                    Foreground="{StaticResource TextPrimaryBrush}"
                    BorderBrush="{StaticResource CellBorderBrush}" />
            <Button x:Name="ExitButton"
                    Content="Exit"
                    Command="{Binding ExitCommand}"
                    Height="40"
                    Margin="0,10,0,0"
                    Background="{StaticResource CellHoverBrush}"
                    Foreground="{StaticResource TextPrimaryBrush}"
                    BorderBrush="{StaticResource CellBorderBrush}" />
        </StackPanel>
    </Border>
</Grid>
```

- [x] **Step 5: Run menu smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [x] **Step 6: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: add main menu screen"
```

---

## Task 4: Move Settings To Separate Screen

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing settings screen smoke test**

Add this test to `tests/ColorLines.Tests/WpfSmokeTests.cs` after `MainMenuShowsActionsBeforeGameplay`:

```csharp
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
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter SettingsScreenUsesExistingGameSettingsCommands
```

Expected: FAIL because `SettingsView` does not exist and the old inline settings panel still exists.

- [x] **Step 3: Remove inline `SettingsPanel` from gameplay**

Delete the gameplay `Border x:Name="SettingsPanel"` block from `MainWindow.xaml`.

Add a compact gameplay settings button below `NewGameButton`:

```xml
<Button x:Name="GameplaySettingsButton"
        Content="Settings"
        Command="{Binding OpenSettingsCommand}"
        Height="36"
        Margin="0,10,0,0"
        Background="{StaticResource CellHoverBrush}"
        Foreground="{StaticResource TextPrimaryBrush}"
        BorderBrush="{StaticResource CellBorderBrush}" />
```

- [x] **Step 4: Add `SettingsView` after `GameplayView`**

Add this grid after the gameplay grid and before `GameOverOverlay`:

```xml
<Grid x:Name="SettingsView"
      Margin="28"
      Visibility="{Binding IsSettingsVisible, Converter={StaticResource BooleanToVisibilityConverter}}">
    <Border Width="420"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            Background="{StaticResource PanelBackgroundBrush}"
            BorderBrush="{StaticResource PanelBorderBrush}"
            BorderThickness="1"
            CornerRadius="10"
            Padding="26">
        <StackPanel>
            <TextBlock Text="Settings"
                       FontSize="34"
                       FontWeight="Bold"
                       Foreground="{StaticResource TextPrimaryBrush}" />
            <TextBlock Text="{Binding Game.SelectedThemeName, StringFormat=Theme: {0}}"
                       Margin="0,18,0,0"
                       FontSize="16"
                       Foreground="{StaticResource TextPrimaryBrush}" />
            <TextBlock Text="{Binding Game.AnimationIntensity, StringFormat=Animation: {0}}"
                       Margin="0,8,0,0"
                       FontSize="14"
                       Foreground="{StaticResource TextMutedBrush}" />
            <Button x:Name="SettingsToggleAnimationButton"
                    Content="{Binding Game.AnimationToggleText}"
                    Command="{Binding Game.ToggleAnimationCommand}"
                    Height="40"
                    Margin="0,18,0,0"
                    Background="{StaticResource CellHoverBrush}"
                    Foreground="{StaticResource TextPrimaryBrush}"
                    BorderBrush="{StaticResource CellBorderBrush}" />
            <Button x:Name="SettingsToggleSoundButton"
                    Content="Toggle Sound"
                    Command="{Binding Game.ToggleSoundCommand}"
                    Height="40"
                    Margin="0,10,0,0"
                    Background="{StaticResource CellHoverBrush}"
                    Foreground="{StaticResource TextPrimaryBrush}"
                    BorderBrush="{StaticResource CellBorderBrush}" />
            <TextBlock Text="{Binding Game.IsSoundEnabled, StringFormat=Sound enabled: {0}}"
                       Margin="0,8,0,0"
                       FontSize="13"
                       Foreground="{StaticResource TextMutedBrush}" />
            <Button x:Name="SettingsBackToMenuButton"
                    Content="Back"
                    Command="{Binding BackToMenuCommand}"
                    Height="42"
                    Margin="0,22,0,0"
                    Background="{StaticResource AccentBrush}"
                    Foreground="#FFFFFB"
                    BorderThickness="0"
                    FontSize="15"
                    FontWeight="SemiBold" />
        </StackPanel>
    </Border>
</Grid>
```

- [x] **Step 5: Run settings smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter SettingsScreenUsesExistingGameSettingsCommands
```

Expected: PASS.

- [x] **Step 6: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: move settings to separate screen"
```

---

## Task 5: Update Gameplay Smoke Tests And Overlay Visibility

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Update `OccupiedCellsShowPieceBody` for shell flow**

In `OccupiedCellsShowPieceBody`, after `window.UpdateLayout();`, add:

```csharp
var shell = Assert.IsType<ShellViewModel>(window.DataContext);
shell.ContinueCommand.Execute(null);
window.UpdateLayout();
```

Remove the lookup and assertion for `SettingsPanel`.

Update bindings expected by lookups:

- `ToggleAnimationButton` is no longer in gameplay, so remove its lookup and assertion.
- Keep `NewGameButton`, `ScorePanel`, `NextCatsPanel`, board, piece, and overlay text checks.

- [x] **Step 2: Update `SelectingOccupiedCellDoesNotCrashAnimationTemplate`**

After `window.UpdateLayout();`, add:

```csharp
var shell = Assert.IsType<ShellViewModel>(window.DataContext);
shell.ContinueCommand.Execute(null);
window.UpdateLayout();
```

- [x] **Step 3: Update overlay XML test**

In `GameOverOverlayIsOutsideContentMargin`, change the `contentGrid` lookup to `gameplayView`:

```csharp
var gameplayView = rootGrid?
    .Elements()
    .First(element => element.Attribute(x + "Name")?.Value == "GameplayView");
```

Change the assertion:

```csharp
Assert.Equal("28", gameplayView?.Attribute("Margin")?.Value);
```

- [x] **Step 4: Gate game-over overlay to gameplay visibility**

In `MainWindow.xaml`, update `GameOverOverlay` visibility so it only appears on the playing screen. Add this style to the overlay and remove the direct `Visibility="{Binding Game.IsGameOver, ...}"` attribute:

```xml
<Border x:Name="GameOverOverlay"
        Background="#99000000">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Visibility" Value="Collapsed" />
            <Style.Triggers>
                <MultiDataTrigger>
                    <MultiDataTrigger.Conditions>
                        <Condition Binding="{Binding IsPlayingVisible}" Value="True" />
                        <Condition Binding="{Binding Game.IsGameOver}" Value="True" />
                    </MultiDataTrigger.Conditions>
                    <Setter Property="Visibility" Value="Visible" />
                </MultiDataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
```

Update game-over bindings with the `Game.` prefix:

- `GameOverTitle`
- `GameOverSummaryText`
- `FinalScoreText`
- `BestScoreText`

Keep the dialog New Game button bound to shell `NewGameCommand`.

- [x] **Step 5: Run WPF smoke tests**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "OccupiedCellsShowPieceBody|SelectingOccupiedCellDoesNotCrashAnimationTemplate|GameOverOverlayIsOutsideContentMargin"
```

Expected: PASS.

- [x] **Step 6: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "fix: adapt gameplay smoke tests for shell"
```

---

## Task 6: Full Verification And Launch

**Files:**
- Modify: `docs/superpowers/plans/2026-05-19-color-lines-main-menu-shell.md`

- [x] **Step 1: Run full tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: PASS.

- [x] **Step 2: Run full build**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: PASS with 0 errors.

- [x] **Step 3: Launch Windows app**

Run:

```powershell
Start-Process -FilePath (Resolve-Path 'src\ColorLines.Windows\bin\Debug\net8.0-windows\ColorLines.Windows.exe').Path -WorkingDirectory (Resolve-Path '.').Path -PassThru | Select-Object Id,ProcessName
```

Expected: the app launches to the main menu. Continue enters the board. Settings opens separately. The game screen no longer has the old large settings panel.

- [x] **Step 4: Mark plan complete**

Replace every unchecked checkbox in this file with a checked checkbox.

- [x] **Step 5: Commit**

```powershell
git add docs\superpowers\plans\2026-05-19-color-lines-main-menu-shell.md
git commit -m "docs: mark main menu shell complete"
```

---

## Self-Review

- Spec coverage: main menu is Task 3, gameplay focus and removed inline settings are Tasks 3-5, separate settings screen is Task 4, shell view model is Tasks 1-2, save behavior is Task 2, full verification is Task 6.
- Placeholder scan: no placeholder markers remain.
- Type consistency: `ShellViewModel`, `ShellScreen`, and all named XAML controls are introduced before tests rely on them.

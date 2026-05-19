# Color Lines Gameplay HUD Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the current desktop-like gameplay right rail with a compact, board-first game HUD while preserving all existing gameplay behavior.

**Architecture:** Keep the existing WPF shell and `GameplayView`, but rename and restructure the view's internal regions into a board area plus compact HUD panel. Add small theme resources and button styles in the existing theme/resource locations, then update WPF smoke tests to protect the new visual structure and command bindings.

**Tech Stack:** .NET 8, WPF XAML, xUnit WPF smoke tests.

---

## File Structure

- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: update gameplay layout assertions from `RightRail` to the new HUD region names and command checks.
- Modify `src/ColorLines.Windows/Themes/CozyBoard.xaml`: add gameplay HUD color/brush resources.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: add compact gameplay button styles and restructure `GameplayView`.
- Verify with `dotnet test ColorLines.sln` and `dotnet build ColorLines.sln`.

## Task 1: Protect the New Gameplay HUD Structure

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Replace old right-rail test lookups with HUD region lookups**

In `OccupiedCellsShowPieceBody`, replace the lookup block:

```csharp
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
```

with:

```csharp
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
var mainBoardFrame = FindVisualChildren<Border>(window)
    .First(border => border.Name == "MainBoardFrame");
var menuButton = FindVisualChildren<Button>(window)
    .First(button => button.Name == "GameplayMenuButton");
var newGameButton = FindVisualChildren<Button>(window)
    .First(button => button.Name == "NewGameButton");
var settingsButton = FindVisualChildren<Button>(window)
    .First(button => button.Name == "GameplaySettingsButton");
```

- [ ] **Step 2: Replace old layout assertions with HUD assertions**

Replace these assertions:

```csharp
Assert.True(statusTextRegion.MinHeight >= 54);
Assert.True(mainBoardFrame.Padding.Left >= 18);
Assert.True(rightRail.Margin.Left >= 24);
Assert.True(scorePanel.Padding.Left >= 16);
Assert.True(nextCatsPanel.Padding.Left >= 16);
Assert.NotNull(newGameButton.Command);
```

with:

```csharp
Assert.Equal(Visibility.Visible, gameplayShell.Visibility);
Assert.True(gameplayBoardArea.ColumnDefinitions.Count >= 1);
Assert.True(gameplayHudPanel.Padding.Left >= 18);
Assert.True(gameplayHudPanel.MaxWidth <= 340);
Assert.True(gameplayStatusBanner.MinHeight >= 58);
Assert.True(gameplayScoreBlock.Padding.Left >= 16);
Assert.True(gameplayNextCatsBlock.Padding.Left >= 16);
Assert.True(gameplayActionBar.Children.Count >= 3);
Assert.True(mainBoardFrame.Padding.Left >= 18);
Assert.NotNull(menuButton.Command);
Assert.NotNull(newGameButton.Command);
Assert.NotNull(settingsButton.Command);
Assert.Equal("MenuSecondaryButton", menuButton.Tag);
Assert.Equal("MenuPrimaryButton", newGameButton.Tag);
Assert.Equal("MenuSecondaryButton", settingsButton.Tag);
```

- [ ] **Step 3: Run the targeted test and confirm it fails before implementation**

Run:

```powershell
dotnet test ColorLines.sln --filter OccupiedCellsShowPieceBody
```

Expected: fail because the XAML still contains `RightRail` and does not yet define the new gameplay HUD region names.

- [ ] **Step 4: Commit the failing test**

Run:

```powershell
git add tests/ColorLines.Tests/WpfSmokeTests.cs
git commit -m "test: protect gameplay hud structure"
```

## Task 2: Add Gameplay HUD Theme Resources and Button Style

**Files:**
- Modify: `src/ColorLines.Windows/Themes/CozyBoard.xaml`
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Add gameplay HUD colors and brushes**

In `CozyBoard.xaml`, after the existing menu button colors, add:

```xml
<Color x:Key="GameplayHudColor">#FFFFFCF4</Color>
<Color x:Key="GameplayHudAccentColor">#FFFFE4B8</Color>
<Color x:Key="GameplayHudMutedColor">#FFF5DDB7</Color>
```

After the existing menu button brushes, add:

```xml
<SolidColorBrush x:Key="GameplayHudBrush" Color="{StaticResource GameplayHudColor}" />
<SolidColorBrush x:Key="GameplayHudAccentBrush" Color="{StaticResource GameplayHudAccentColor}" />
<SolidColorBrush x:Key="GameplayHudMutedBrush" Color="{StaticResource GameplayHudMutedColor}" />
```

- [ ] **Step 2: Add a compact gameplay button style**

In `MainWindow.xaml` inside `Window.Resources`, after `MenuSecondaryButton`, add:

```xml
<Style x:Key="GameplayCompactButton" TargetType="Button" BasedOn="{StaticResource MenuSecondaryButton}">
    <Setter Property="MinHeight" Value="38" />
    <Setter Property="Padding" Value="14,8" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Tag" Value="MenuSecondaryButton" />
</Style>
```

- [ ] **Step 3: Run a resource smoke test**

Run:

```powershell
dotnet test ColorLines.sln --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: pass. The existing test does not need to assert these new brushes because Task 1 protects the actual HUD structure.

- [ ] **Step 4: Commit theme/style resources**

Run:

```powershell
git add src/ColorLines.Windows/Themes/CozyBoard.xaml src/ColorLines.Windows/MainWindow.xaml
git commit -m "feat: add gameplay hud theme resources"
```

## Task 3: Restructure GameplayView into Board Area and HUD Panel

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Replace the outer `GameplayView` grid structure**

Change the opening of `GameplayView` from:

```xml
<Grid x:Name="GameplayView"
      Margin="28"
      Visibility="{Binding IsPlayingVisible, Converter={StaticResource BooleanToVisibilityConverter}}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="300" />
    </Grid.ColumnDefinitions>

    <Border x:Name="MainBoardFrame"
            Grid.Column="0"
```

to:

```xml
<Grid x:Name="GameplayView"
      Margin="28"
      Visibility="{Binding IsPlayingVisible, Converter={StaticResource BooleanToVisibilityConverter}}">
    <Grid x:Name="GameplayShell">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="320" />
        </Grid.ColumnDefinitions>

        <Grid x:Name="GameplayBoardArea"
              Grid.Column="0"
              Margin="0,0,24,0">
            <Border x:Name="MainBoardFrame"
```

Then close the extra `GameplayBoardArea` grid after the board frame closes.

- [ ] **Step 2: Replace the old `RightRail` stack with `GameplayHudPanel`**

Replace the old block beginning with:

```xml
<StackPanel x:Name="RightRail"
            Grid.Column="1"
            Margin="28,0,0,0">
```

through its closing `</StackPanel>` with:

```xml
<Border x:Name="GameplayHudPanel"
        Grid.Column="1"
        MaxWidth="320"
        Background="{StaticResource GameplayHudBrush}"
        BorderBrush="{StaticResource PanelBorderBrush}"
        BorderThickness="1"
        CornerRadius="14"
        Padding="20"
        VerticalAlignment="Stretch">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="14" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="14" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <Border x:Name="GameplayStatusBanner"
                Grid.Row="0"
                MinHeight="58"
                Background="{StaticResource GameplayHudAccentBrush}"
                CornerRadius="10"
                Padding="14,10">
            <TextBlock Text="{Binding Game.StatusText}"
                       FontSize="15"
                       LineHeight="19"
                       TextWrapping="Wrap"
                       Foreground="{StaticResource TextPrimaryBrush}" />
        </Border>

        <Border x:Name="GameplayScoreBlock"
                Grid.Row="2"
                Background="{StaticResource PanelBackgroundBrush}"
                BorderBrush="{StaticResource PanelBorderBrush}"
                BorderThickness="1"
                CornerRadius="10"
                Padding="18,16">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <StackPanel>
                    <TextBlock Text="Score"
                               FontSize="13"
                               FontWeight="SemiBold"
                               Foreground="{StaticResource TextMutedBrush}" />
                    <TextBlock Text="{Binding Game.Score}"
                               FontSize="42"
                               FontWeight="Bold"
                               Foreground="{StaticResource TextPrimaryBrush}" />
                    <TextBlock Text="{Binding Game.HighScore, StringFormat=Best: {0}}"
                               FontSize="14"
                               FontWeight="SemiBold"
                               Foreground="{StaticResource TextMutedBrush}" />
                </StackPanel>
                <Border x:Name="ScoreDeltaBadge"
                        Grid.Column="1"
                        MinWidth="52"
                        Height="40"
                        Background="{StaticResource GameplayHudAccentBrush}"
                        CornerRadius="10"
                        VerticalAlignment="Bottom"
                        Opacity="{Binding Game.ScoreDeltaBadgeOpacity}">
                    <TextBlock Text="{Binding Game.LastScoreDelta, StringFormat=+{0}}"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               FontSize="21"
                               FontWeight="Bold"
                               Foreground="{StaticResource ScoreAccentBrush}" />
                </Border>
            </Grid>
        </Border>

        <Border x:Name="GameplayNextCatsBlock"
                Grid.Row="4"
                Background="{StaticResource PanelBackgroundBrush}"
                BorderBrush="{StaticResource PanelBorderBrush}"
                BorderThickness="1"
                CornerRadius="10"
                Padding="18,16">
            <StackPanel>
                <TextBlock Text="Next Cats"
                           FontSize="15"
                           FontWeight="SemiBold"
                           Foreground="{StaticResource TextMutedBrush}" />
                <ItemsControl ItemsSource="{Binding Game.NextPieces}" Margin="0,12,0,0">
                    <ItemsControl.ItemsPanel>
                        <ItemsPanelTemplate>
                            <StackPanel Orientation="Horizontal" />
                        </ItemsPanelTemplate>
                    </ItemsControl.ItemsPanel>
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Width="64"
                                    Height="64"
                                    Margin="0,0,10,0"
                                    Background="{StaticResource GameplayHudAccentBrush}"
                                    CornerRadius="14">
                                <Image Source="{Binding AssetPath}"
                                       Width="48"
                                       Height="48"
                                       Stretch="Uniform"
                                       RenderOptions.BitmapScalingMode="HighQuality" />
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </Border>

        <StackPanel x:Name="GameplayActionBar"
                    Grid.Row="6">
            <Button x:Name="GameplayMenuButton"
                    Content="Menu"
                    Command="{Binding BackToMenuCommand}"
                    Style="{StaticResource GameplayCompactButton}" />
            <Button x:Name="NewGameButton"
                    Content="New Game"
                    Command="{Binding NewGameCommand}"
                    Style="{StaticResource MenuPrimaryButton}"
                    Margin="0,10,0,0" />
            <Button x:Name="GameplaySettingsButton"
                    Content="Settings"
                    Command="{Binding OpenSettingsCommand}"
                    Style="{StaticResource GameplayCompactButton}"
                    Margin="0,10,0,0" />
        </StackPanel>
    </Grid>
</Border>
```

- [ ] **Step 3: Remove old gameplay title/settings block**

Ensure the old gameplay-only title `Color Lines`, `StatusTextRegion`, `ScorePanel`, `NextCatsPanel`, and inline settings block are no longer present in `GameplayView`. The dedicated settings screen remains unchanged.

- [ ] **Step 4: Run the targeted WPF test**

Run:

```powershell
dotnet test ColorLines.sln --filter OccupiedCellsShowPieceBody
```

Expected: pass.

- [ ] **Step 5: Commit gameplay layout restructure**

Run:

```powershell
git add src/ColorLines.Windows/MainWindow.xaml
git commit -m "feat: polish gameplay hud layout"
```

## Task 4: Full Verification

**Files:**
- No planned source modifications unless verification exposes a bug.

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: all tests pass.

- [ ] **Step 2: Build the solution**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: build succeeds with 0 errors.

- [ ] **Step 3: Launch the app for manual playtest**

Run:

```powershell
Start-Process -FilePath 'dotnet' -ArgumentList 'run --project src/ColorLines.Windows/ColorLines.Windows.csproj' -WorkingDirectory (Resolve-Path '.').Path -WindowStyle Hidden -PassThru | Select-Object Id,ProcessName
```

Expected: app launches. Manually verify that Continue enters the new gameplay HUD, Menu returns to the main menu, New Game starts a new run, Settings opens the settings screen, and the game-over overlay still covers the full content area.

- [ ] **Step 4: Commit verification docs if needed**

If implementation required any plan or doc status update, commit it:

```powershell
git add docs/superpowers/plans/2026-05-19-color-lines-gameplay-hud-polish.md
git commit -m "docs: mark gameplay hud polish complete"
```

If no doc status update is made, skip this step.

# Color Lines Settings Navigation Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Polish the settings screen into a dedicated game-style settings lobby while preserving existing commands and shell navigation.

**Architecture:** Keep `SettingsView` as a separate shell screen and replace its narrow form card with named WPF regions for header, option list, setting rows, and action bar. Reuse existing menu/gameplay brushes and button styles, add only small settings-specific theme resources, and update WPF smoke tests to protect the structure and command bindings.

**Tech Stack:** .NET 8, WPF XAML, xUnit WPF smoke tests.

---

## File Structure

- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: expand `SettingsScreenUsesExistingGameSettingsCommands` to require the new settings lobby structure and exact command bindings.
- Modify `src/ColorLines.Windows/Themes/CozyBoard.xaml`: add small settings surface brushes.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: replace the current centered settings card with the settings lobby layout.
- Verify with `dotnet test ColorLines.sln` and `dotnet build ColorLines.sln`.

## Task 1: Protect the Settings Lobby Structure

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Add settings region lookups**

In `SettingsScreenUsesExistingGameSettingsCommands`, after the `settingsView` lookup, add:

```csharp
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
var settingsActionBar = FindVisualChildren<StackPanel>(window)
    .First(panel => panel.Name == "SettingsActionBar");
```

- [ ] **Step 2: Add New Game button lookup**

After the existing `backToMenuButton` lookup, add:

```csharp
var settingsNewGameButton = FindVisualChildren<Button>(window)
    .First(button => button.Name == "SettingsNewGameButton");
```

- [ ] **Step 3: Add layout and command assertions**

After `Assert.Equal(Visibility.Visible, settingsView.Visibility);`, add:

```csharp
Assert.Equal(Visibility.Visible, settingsShell.Visibility);
Assert.True(settingsHeader.Children.Count >= 2);
Assert.True(settingsContentPanel.Padding.Left >= 24);
Assert.True(settingsOptionList.Children.Count >= 2);
Assert.True(animationSettingRow.MinHeight >= 82);
Assert.True(soundSettingRow.MinHeight >= 82);
Assert.True(settingsActionBar.Children.Count >= 2);
Assert.Equal("MenuSecondaryButton", settingsToggleAnimationButton.Tag);
Assert.Equal("MenuSecondaryButton", settingsToggleSoundButton.Tag);
Assert.Equal("MenuPrimaryButton", settingsNewGameButton.Tag);
Assert.Equal("MenuSecondaryButton", backToMenuButton.Tag);
```

Replace the existing back command assertion block with:

```csharp
Assert.Same(shell.Game.ToggleAnimationCommand, settingsToggleAnimationButton.Command);
Assert.Same(shell.Game.ToggleSoundCommand, settingsToggleSoundButton.Command);
Assert.Same(shell.NewGameCommand, settingsNewGameButton.Command);
Assert.Same(shell.BackToMenuCommand, backToMenuButton.Command);
```

- [ ] **Step 4: Run the targeted test and confirm it fails before implementation**

Run:

```powershell
dotnet test ColorLines.sln --filter SettingsScreenUsesExistingGameSettingsCommands
```

Expected: fail because the current XAML does not define `SettingsShell` and related settings lobby regions.

- [ ] **Step 5: Commit the failing test**

Run:

```powershell
git add tests/ColorLines.Tests/WpfSmokeTests.cs
git commit -m "test: protect settings lobby structure"
```

## Task 2: Add Settings Theme Resources

**Files:**
- Modify: `src/ColorLines.Windows/Themes/CozyBoard.xaml`

- [ ] **Step 1: Add settings colors**

In `CozyBoard.xaml`, after the gameplay HUD colors, add:

```xml
<Color x:Key="SettingsSurfaceColor">#FFFFFBF2</Color>
<Color x:Key="SettingsRowColor">#FFFFF4DC</Color>
```

- [ ] **Step 2: Add settings brushes**

After the gameplay HUD brushes, add:

```xml
<SolidColorBrush x:Key="SettingsSurfaceBrush" Color="{StaticResource SettingsSurfaceColor}" />
<SolidColorBrush x:Key="SettingsRowBrush" Color="{StaticResource SettingsRowColor}" />
```

- [ ] **Step 3: Run a theme smoke test**

Run:

```powershell
dotnet test ColorLines.sln --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: pass. The dedicated settings structure test will cover usage of the new resources.

- [ ] **Step 4: Commit theme resources**

Run:

```powershell
git add src/ColorLines.Windows/Themes/CozyBoard.xaml
git commit -m "feat: add settings lobby theme resources"
```

## Task 3: Replace SettingsView with a Settings Lobby Layout

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Replace the current settings card**

Replace the current `SettingsView` child border and stack panel with:

```xml
<Grid x:Name="SettingsShell">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="520" />
    </Grid.ColumnDefinitions>

    <StackPanel x:Name="SettingsHeader"
                Grid.Column="0"
                VerticalAlignment="Center"
                Margin="34,0,42,0">
        <TextBlock Text="Settings"
                   FontSize="48"
                   FontWeight="Bold"
                   Foreground="{StaticResource TextPrimaryBrush}" />
        <TextBlock Text="Tune the table before the next run."
                   Margin="0,10,0,0"
                   FontSize="18"
                   TextWrapping="Wrap"
                   Foreground="{StaticResource TextMutedBrush}" />
        <Border Margin="0,28,0,0"
                Background="{StaticResource GameplayHudAccentBrush}"
                CornerRadius="12"
                Padding="18,14"
                MaxWidth="360"
                HorizontalAlignment="Left">
            <StackPanel>
                <TextBlock Text="{Binding Game.SelectedThemeName, StringFormat=Theme: {0}}"
                           FontSize="17"
                           FontWeight="SemiBold"
                           Foreground="{StaticResource TextPrimaryBrush}" />
                <TextBlock Text="{Binding Game.HighScore, StringFormat=Best Score: {0}}"
                           Margin="0,6,0,0"
                           FontSize="14"
                           Foreground="{StaticResource TextMutedBrush}" />
            </StackPanel>
        </Border>
    </StackPanel>

    <Border x:Name="SettingsContentPanel"
            Grid.Column="1"
            Background="{StaticResource SettingsSurfaceBrush}"
            BorderBrush="{StaticResource PanelBorderBrush}"
            BorderThickness="1"
            CornerRadius="14"
            Padding="28"
            VerticalAlignment="Center">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="24" />
                <RowDefinition Height="Auto" />
            </Grid.RowDefinitions>

            <StackPanel x:Name="SettingsOptionList">
                <Border x:Name="AnimationSettingRow"
                        MinHeight="84"
                        Background="{StaticResource SettingsRowBrush}"
                        BorderBrush="{StaticResource PanelBorderBrush}"
                        BorderThickness="1"
                        CornerRadius="10"
                        Padding="18,14">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="160" />
                        </Grid.ColumnDefinitions>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Animation"
                                       FontSize="18"
                                       FontWeight="SemiBold"
                                       Foreground="{StaticResource TextPrimaryBrush}" />
                            <TextBlock Text="{Binding Game.AnimationIntensity, StringFormat=Current: {0}}"
                                       Margin="0,5,0,0"
                                       FontSize="14"
                                       Foreground="{StaticResource TextMutedBrush}" />
                        </StackPanel>
                        <Button x:Name="SettingsToggleAnimationButton"
                                Grid.Column="1"
                                Content="{Binding Game.AnimationToggleText}"
                                Command="{Binding Game.ToggleAnimationCommand}"
                                Style="{StaticResource GameplayCompactButton}"
                                VerticalAlignment="Center" />
                    </Grid>
                </Border>

                <Border x:Name="SoundSettingRow"
                        MinHeight="84"
                        Margin="0,12,0,0"
                        Background="{StaticResource SettingsRowBrush}"
                        BorderBrush="{StaticResource PanelBorderBrush}"
                        BorderThickness="1"
                        CornerRadius="10"
                        Padding="18,14">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="160" />
                        </Grid.ColumnDefinitions>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Sound"
                                       FontSize="18"
                                       FontWeight="SemiBold"
                                       Foreground="{StaticResource TextPrimaryBrush}" />
                            <TextBlock Text="{Binding Game.IsSoundEnabled, StringFormat=Enabled: {0}}"
                                       Margin="0,5,0,0"
                                       FontSize="14"
                                       Foreground="{StaticResource TextMutedBrush}" />
                        </StackPanel>
                        <Button x:Name="SettingsToggleSoundButton"
                                Grid.Column="1"
                                Content="Toggle Sound"
                                Command="{Binding Game.ToggleSoundCommand}"
                                Style="{StaticResource GameplayCompactButton}"
                                VerticalAlignment="Center" />
                    </Grid>
                </Border>
            </StackPanel>

            <StackPanel x:Name="SettingsActionBar"
                        Grid.Row="2">
                <Button x:Name="SettingsNewGameButton"
                        Content="New Game"
                        Command="{Binding NewGameCommand}"
                        Style="{StaticResource MenuPrimaryButton}" />
                <Button x:Name="SettingsBackToMenuButton"
                        Content="Back to Menu"
                        Command="{Binding BackToMenuCommand}"
                        Style="{StaticResource GameplayCompactButton}"
                        Margin="0,10,0,0" />
            </StackPanel>
        </Grid>
    </Border>
</Grid>
```

- [ ] **Step 2: Ensure old raw button styling is gone**

Confirm `SettingsToggleAnimationButton`, `SettingsToggleSoundButton`, and `SettingsBackToMenuButton` use styles instead of raw `Background`, `Foreground`, `BorderBrush`, `Height`, and `FontWeight` settings.

- [ ] **Step 3: Run the targeted settings test**

Run:

```powershell
dotnet test ColorLines.sln --filter SettingsScreenUsesExistingGameSettingsCommands
```

Expected: pass.

- [ ] **Step 4: Commit settings layout**

Run:

```powershell
git add src/ColorLines.Windows/MainWindow.xaml
git commit -m "feat: polish settings lobby layout"
```

## Task 4: Full Verification and Launch

**Files:**
- No planned source modifications unless verification exposes a bug.

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: all 82 tests pass or the updated total passes if tests were added.

- [ ] **Step 2: Build the solution**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: build succeeds with 0 errors.

- [ ] **Step 3: Launch the app for manual inspection**

Run:

```powershell
Start-Process -FilePath 'dotnet' -ArgumentList 'run --project src/ColorLines.Windows/ColorLines.Windows.csproj' -WorkingDirectory (Resolve-Path '.').Path -WindowStyle Hidden -PassThru | Select-Object Id,ProcessName
```

Expected: app launches. Manually verify that main menu Settings opens the settings lobby, gameplay Settings opens the same settings lobby, Back to Menu returns to the main menu, New Game starts a fresh game, and the setting rows do not jump when toggled.

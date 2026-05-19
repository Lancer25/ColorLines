# Color Lines Main Menu Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Polish the existing tabletop main menu into a more finished modern casual-game title screen.

**Architecture:** Keep `ShellViewModel` and navigation behavior unchanged. Add reusable menu brushes and button styles to WPF resources, then update `MainMenuView` with named backdrop, hero, board, command, and status structures protected by smoke tests.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## File Structure

- Modify `src/ColorLines.Windows/Themes/CozyBoard.xaml`: add menu-specific brushes for tabletop background and command buttons.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: add menu button styles and restyle `MainMenuView`.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: protect the new menu backdrop, hero area, button structure, and status strip.
- Modify `docs/superpowers/plans/2026-05-19-color-lines-main-menu-polish.md`: mark tasks complete.

---

## Task 1: Add Menu Theme Resources

**Files:**
- Modify: `src/ColorLines.Windows/Themes/CozyBoard.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing resource smoke test assertions**

In `CozyBoardThemeExposesVisualUpgradeBrushes`, add:

```csharp
Assert.True(Application.Current.Resources.Contains("MenuBackdropBrush"));
Assert.True(Application.Current.Resources.Contains("MenuPrimaryButtonBrush"));
Assert.True(Application.Current.Resources.Contains("MenuPrimaryButtonHoverBrush"));
Assert.True(Application.Current.Resources.Contains("MenuSecondaryButtonBrush"));
Assert.True(Application.Current.Resources.Contains("MenuSecondaryButtonHoverBrush"));
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: FAIL because the new menu resources do not exist yet.

- [x] **Step 3: Add menu resources**

In `src/ColorLines.Windows/Themes/CozyBoard.xaml`, add these colors after the current color keys:

```xml
<Color x:Key="MenuPrimaryButtonColor">#8F5834</Color>
<Color x:Key="MenuPrimaryButtonHoverColor">#A8653B</Color>
<Color x:Key="MenuPrimaryButtonPressedColor">#6B3F25</Color>
<Color x:Key="MenuSecondaryButtonColor">#FFFFF2D6</Color>
<Color x:Key="MenuSecondaryButtonHoverColor">#FFFFE4B8</Color>
<Color x:Key="MenuSecondaryButtonPressedColor">#FFE9C88F</Color>
```

Add these brushes after the current brush keys:

```xml
<LinearGradientBrush x:Key="MenuBackdropBrush" StartPoint="0,0" EndPoint="1,1">
    <GradientStop Color="#FFF7E5C9" Offset="0" />
    <GradientStop Color="#FFEBC190" Offset="0.55" />
    <GradientStop Color="#FFD19A64" Offset="1" />
</LinearGradientBrush>
<SolidColorBrush x:Key="MenuPrimaryButtonBrush" Color="{StaticResource MenuPrimaryButtonColor}" />
<SolidColorBrush x:Key="MenuPrimaryButtonHoverBrush" Color="{StaticResource MenuPrimaryButtonHoverColor}" />
<SolidColorBrush x:Key="MenuPrimaryButtonPressedBrush" Color="{StaticResource MenuPrimaryButtonPressedColor}" />
<SolidColorBrush x:Key="MenuSecondaryButtonBrush" Color="{StaticResource MenuSecondaryButtonColor}" />
<SolidColorBrush x:Key="MenuSecondaryButtonHoverBrush" Color="{StaticResource MenuSecondaryButtonHoverColor}" />
<SolidColorBrush x:Key="MenuSecondaryButtonPressedBrush" Color="{StaticResource MenuSecondaryButtonPressedColor}" />
```

- [x] **Step 4: Run resource smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\Themes\CozyBoard.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: add main menu polish theme resources"
```

---

## Task 2: Add Reusable Menu Button Styles

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing button style assertions**

In `MainMenuShowsActionsBeforeGameplay`, after the existing button height assertions, add:

```csharp
Assert.Equal("MenuPrimaryButton", continueButton.Style.Resources["StyleKey"]);
Assert.Equal("MenuSecondaryButton", menuSettingsButton.Style.Resources["StyleKey"]);
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because the styles do not exist yet.

- [x] **Step 3: Add button styles**

In `MainWindow.xaml` `Window.Resources`, after `BoardCellButton`, add:

```xml
<Style x:Key="MenuPrimaryButton" TargetType="Button">
    <Style.Resources>
        <x:String x:Key="StyleKey">MenuPrimaryButton</x:String>
    </Style.Resources>
    <Setter Property="Height" Value="54" />
    <Setter Property="Padding" Value="18,0" />
    <Setter Property="Background" Value="{StaticResource MenuPrimaryButtonBrush}" />
    <Setter Property="Foreground" Value="#FFFFFB" />
    <Setter Property="BorderBrush" Value="{StaticResource MenuPrimaryButtonHoverBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="FontSize" Value="16" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="ButtonFrame"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="8">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="ButtonFrame" Property="Background" Value="{StaticResource MenuPrimaryButtonHoverBrush}" />
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="ButtonFrame" Property="Background" Value="{StaticResource MenuPrimaryButtonPressedBrush}" />
                    </Trigger>
                    <Trigger Property="IsKeyboardFocused" Value="True">
                        <Setter TargetName="ButtonFrame" Property="BorderBrush" Value="#FFFFE4B8" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<Style x:Key="MenuSecondaryButton" TargetType="Button">
    <Style.Resources>
        <x:String x:Key="StyleKey">MenuSecondaryButton</x:String>
    </Style.Resources>
    <Setter Property="Height" Value="46" />
    <Setter Property="Padding" Value="16,0" />
    <Setter Property="Background" Value="{StaticResource MenuSecondaryButtonBrush}" />
    <Setter Property="Foreground" Value="{StaticResource TextPrimaryBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource CellBorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="FontSize" Value="14" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="ButtonFrame"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="8">
                    <ContentPresenter HorizontalAlignment="Center"
                                      VerticalAlignment="Center" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="ButtonFrame" Property="Background" Value="{StaticResource MenuSecondaryButtonHoverBrush}" />
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="ButtonFrame" Property="Background" Value="{StaticResource MenuSecondaryButtonPressedBrush}" />
                    </Trigger>
                    <Trigger Property="IsKeyboardFocused" Value="True">
                        <Setter TargetName="ButtonFrame" Property="BorderBrush" Value="{StaticResource MenuPrimaryButtonHoverBrush}" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

Update menu buttons:

```xml
Style="{StaticResource MenuPrimaryButton}"
```

for `ContinueButton`, and:

```xml
Style="{StaticResource MenuSecondaryButton}"
```

for `MenuNewGameButton`, `MenuSettingsButton`, and `ExitButton`.

Remove duplicated explicit button `Height`, `Background`, `Foreground`, `BorderBrush`, `BorderThickness`, `FontSize`, and `FontWeight` attributes from those four buttons.

- [x] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: add game menu button styles"
```

---

## Task 3: Add Backdrop And Hero Area Composition

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add failing backdrop and hero assertions**

In `MainMenuShowsActionsBeforeGameplay`, add these lookups:

```csharp
var menuBackdrop = FindVisualChildren<Border>(window)
    .First(border => border.Name == "MenuBackdrop");
var menuHeroArea = FindVisualChildren<Grid>(window)
    .First(grid => grid.Name == "MenuHeroArea");
```

Add assertions:

```csharp
Assert.Equal(Visibility.Visible, menuBackdrop.Visibility);
Assert.True(menuHeroArea.Margin.Left >= 24);
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because `MenuBackdrop` and `MenuHeroArea` do not exist yet.

- [x] **Step 3: Add backdrop and hero area**

In `MainWindow.xaml`, inside `MainMenuView`, add this as the first child before `Grid.ColumnDefinitions`:

```xml
<Border x:Name="MenuBackdrop"
        Grid.ColumnSpan="2"
        Background="{StaticResource MenuBackdropBrush}"
        CornerRadius="18" />
```

Wrap the existing left-side title and board `StackPanel` in a `Grid x:Name="MenuHeroArea"`:

```xml
<Grid x:Name="MenuHeroArea"
      Grid.Column="0"
      Margin="30,0,44,0"
      VerticalAlignment="Center">
    <StackPanel>
        ...
    </StackPanel>
</Grid>
```

Remove `Grid.Column`, `VerticalAlignment`, and old margin from the inner `StackPanel` so the new `MenuHeroArea` owns positioning.

- [x] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: add tabletop main menu backdrop"
```

---

## Task 4: Polish Hero Board And Status Strip

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] **Step 1: Add status strip sound assertion**

In `MainMenuShowsActionsBeforeGameplay`, update:

```csharp
Assert.True(menuStatusStrip.Children.Count >= 3);
```

to:

```csharp
Assert.True(menuStatusStrip.Children.Count >= 4);
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because the status strip only has best score, theme, and animation.

- [x] **Step 3: Add sound state and polish board frame**

In `MenuStatusStrip`, add:

```xml
<TextBlock Text="{Binding Game.IsSoundEnabled, StringFormat=Sound: {0}}"
           Margin="0,4,0,0"
           FontSize="13"
           Foreground="{StaticResource TextMutedBrush}" />
```

Update `MenuHeroBoard`:

- `CornerRadius="22"`
- `Padding="18"`
- `BorderThickness="2"`

Update inner board border:

- `CornerRadius="16"`
- `Padding="16"`

Update menu title subtitle text from `"Cute cats. Classic lines. One cozy board."` to `"Match five cats. Clear the board. Chase your best score."`

- [x] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: polish menu hero board and status"
```

---

## Task 5: Full Verification And Launch

**Files:**
- Modify: `docs/superpowers/plans/2026-05-19-color-lines-main-menu-polish.md`

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

Expected: the app launches to the polished tabletop title screen.

- [x] **Step 4: Mark plan complete**

Replace every unchecked checkbox in this file with a checked checkbox.

- [x] **Step 5: Commit**

```powershell
git add docs\superpowers\plans\2026-05-19-color-lines-main-menu-polish.md
git commit -m "docs: mark main menu polish complete"
```

---

## Self-Review

- Spec coverage: backdrop is Task 3, hero board is Task 4, custom buttons are Task 2, status strip is Task 4, full verification is Task 5.
- Placeholder scan: no placeholder markers remain.
- Type consistency: resource names, style names, and XAML element names are introduced before tests rely on them.

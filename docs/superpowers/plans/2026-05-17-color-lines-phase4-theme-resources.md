# Color Lines Phase 4 Theme Resources Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish the first `CozyBoard` theme resource system for the WPF game.

**Architecture:** Keep theme metadata and settings in the WPF layer. `ThemeCatalog` exposes available themes and `GameViewModel` exposes selected theme, sound, and animation options for binding. WPF resource dictionaries hold default colors and reusable styles so future themes can replace resources without touching core rules.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Scope

This plan implements Phase 4 basics:

- Add theme metadata model and catalog for `CozyBoard`.
- Add settings state to `GameViewModel`: selected theme, sound enabled, and animation intensity.
- Add WPF theme resource dictionary under `Themes/CozyBoard.xaml`.
- Move primary colors and board/cell/button styles into reusable resources.
- Add a compact settings panel in the right rail.
- Add resource folder placeholders for future board, pieces, effects, and sounds.

This plan does not implement loading external image or sound files. It creates the structure and binding surface for those assets.

## File Structure

Create or modify:

```text
src/ColorLines.Windows/Themes/ThemeInfo.cs
src/ColorLines.Windows/Themes/ThemeCatalog.cs
src/ColorLines.Windows/Themes/CozyBoard.xaml
src/ColorLines.Windows/Assets/Themes/CozyBoard/board/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/effects/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/sounds/.gitkeep
src/ColorLines.Windows/App.xaml
src/ColorLines.Windows/MainWindow.xaml
src/ColorLines.Windows/ViewModels/GameViewModel.cs
tests/ColorLines.Tests/GameViewModelTests.cs
tests/ColorLines.Tests/ThemeCatalogTests.cs
README.md
```

## Task 1: Add Theme Catalog

**Files:**
- Create: `src/ColorLines.Windows/Themes/ThemeInfo.cs`
- Create: `src/ColorLines.Windows/Themes/ThemeCatalog.cs`
- Create: `tests/ColorLines.Tests/ThemeCatalogTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/ColorLines.Tests/ThemeCatalogTests.cs`:

```csharp
using ColorLines.Windows.Themes;

namespace ColorLines.Tests;

public sealed class ThemeCatalogTests
{
    [Fact]
    public void CatalogContainsCozyBoardTheme()
    {
        var theme = ThemeCatalog.DefaultTheme;

        Assert.Equal("CozyBoard", theme.Id);
        Assert.Equal("Cozy Board", theme.DisplayName);
        Assert.Contains("warm", theme.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AvailableThemesReturnsDefaultTheme()
    {
        Assert.Contains(ThemeCatalog.AvailableThemes, theme => theme.Id == ThemeCatalog.DefaultTheme.Id);
    }
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter ThemeCatalogTests
```

Expected: compile fails because `ColorLines.Windows.Themes` does not exist.

- [ ] **Step 3: Add theme metadata**

Create `src/ColorLines.Windows/Themes/ThemeInfo.cs`:

```csharp
namespace ColorLines.Windows.Themes;

public sealed record ThemeInfo(string Id, string DisplayName, string Description);
```

Create `src/ColorLines.Windows/Themes/ThemeCatalog.cs`:

```csharp
namespace ColorLines.Windows.Themes;

public static class ThemeCatalog
{
    public static ThemeInfo DefaultTheme { get; } = new(
        "CozyBoard",
        "Cozy Board",
        "A warm tabletop board with soft cells and round cat pieces.");

    public static IReadOnlyList<ThemeInfo> AvailableThemes { get; } = new[] { DefaultTheme };
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter ThemeCatalogTests
```

Expected: all `ThemeCatalogTests` pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/ColorLines.Windows/Themes/ThemeInfo.cs src/ColorLines.Windows/Themes/ThemeCatalog.cs tests/ColorLines.Tests/ThemeCatalogTests.cs
git commit -m "feat: add theme catalog"
```

## Task 2: Add Settings State To GameViewModel

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests**

Append inside `GameViewModelTests`:

```csharp
[Fact]
public void GameViewModelExposesDefaultThemeAndSettings()
{
    var viewModel = GameViewModel.CreateForNewGame();

    Assert.Equal("Cozy Board", viewModel.SelectedThemeName);
    Assert.True(viewModel.IsSoundEnabled);
    Assert.Equal("Full", viewModel.AnimationIntensity);
}

[Fact]
public void ToggleSoundSwitchesSoundSetting()
{
    var viewModel = GameViewModel.CreateForNewGame();

    viewModel.ToggleSoundCommand.Execute(null);

    Assert.False(viewModel.IsSoundEnabled);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because settings properties and command do not exist.

- [ ] **Step 3: Implement settings state**

In `GameViewModel.cs`, import:

```csharp
using ColorLines.Windows.Themes;
```

Add fields:

```csharp
private bool isSoundEnabled;
private string animationIntensity;
```

Initialize in constructor:

```csharp
isSoundEnabled = true;
animationIntensity = "Full";
ToggleSoundCommand = new RelayCommand(_ => IsSoundEnabled = !IsSoundEnabled);
```

Add property and command:

```csharp
public ICommand ToggleSoundCommand { get; }

public string SelectedThemeName => ThemeCatalog.DefaultTheme.DisplayName;

public bool IsSoundEnabled
{
    get => isSoundEnabled;
    private set
    {
        if (isSoundEnabled != value)
        {
            isSoundEnabled = value;
            OnPropertyChanged();
        }
    }
}

public string AnimationIntensity
{
    get => animationIntensity;
    private set
    {
        if (animationIntensity != value)
        {
            animationIntensity = value;
            OnPropertyChanged();
        }
    }
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/GameViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: add game settings state"
```

## Task 3: Add CozyBoard Resource Dictionary And Asset Folders

**Files:**
- Create: `src/ColorLines.Windows/Themes/CozyBoard.xaml`
- Modify: `src/ColorLines.Windows/App.xaml`
- Create: asset `.gitkeep` files

- [ ] **Step 1: Add theme resource dictionary**

Create `src/ColorLines.Windows/Themes/CozyBoard.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Color x:Key="AppBackgroundColor">#FFF7EF</Color>
    <Color x:Key="PanelBackgroundColor">#FFFFFB</Color>
    <Color x:Key="BoardBackgroundColor">#FFE1BB</Color>
    <Color x:Key="BoardBorderColor">#D79B63</Color>
    <Color x:Key="CellBackgroundColor">#FFF8E7</Color>
    <Color x:Key="CellHoverColor">#FFF0CF</Color>
    <Color x:Key="CellBorderColor">#E8B982</Color>
    <Color x:Key="TextPrimaryColor">#4A2D23</Color>
    <Color x:Key="TextMutedColor">#8A6655</Color>
    <Color x:Key="AccentColor">#7C4D2E</Color>
    <Color x:Key="ScoreAccentColor">#D96B2B</Color>

    <SolidColorBrush x:Key="AppBackgroundBrush" Color="{StaticResource AppBackgroundColor}" />
    <SolidColorBrush x:Key="PanelBackgroundBrush" Color="{StaticResource PanelBackgroundColor}" />
    <SolidColorBrush x:Key="BoardBackgroundBrush" Color="{StaticResource BoardBackgroundColor}" />
    <SolidColorBrush x:Key="BoardBorderBrush" Color="{StaticResource BoardBorderColor}" />
    <SolidColorBrush x:Key="CellBackgroundBrush" Color="{StaticResource CellBackgroundColor}" />
    <SolidColorBrush x:Key="CellHoverBrush" Color="{StaticResource CellHoverColor}" />
    <SolidColorBrush x:Key="CellBorderBrush" Color="{StaticResource CellBorderColor}" />
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}" />
    <SolidColorBrush x:Key="TextMutedBrush" Color="{StaticResource TextMutedColor}" />
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}" />
    <SolidColorBrush x:Key="ScoreAccentBrush" Color="{StaticResource ScoreAccentColor}" />
</ResourceDictionary>
```

- [ ] **Step 2: Merge the dictionary in `App.xaml`**

Replace `src/ColorLines.Windows/App.xaml` with:

```xml
<Application x:Class="ColorLines.Windows.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/CozyBoard.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 3: Add asset folder placeholders**

Create empty `.gitkeep` files:

```text
src/ColorLines.Windows/Assets/Themes/CozyBoard/board/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/effects/.gitkeep
src/ColorLines.Windows/Assets/Themes/CozyBoard/sounds/.gitkeep
```

- [ ] **Step 4: Build and smoke-test WPF window**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter WpfSmokeTests
dotnet build ColorLines.sln
```

Expected: WPF smoke test and build pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/ColorLines.Windows/Themes/CozyBoard.xaml src/ColorLines.Windows/App.xaml src/ColorLines.Windows/Assets
git commit -m "feat: add cozy board theme resources"
```

## Task 4: Bind MainWindow To Theme Resources And Settings

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `README.md`

- [ ] **Step 1: Replace hard-coded primary brushes in `MainWindow.xaml`**

Use these replacements:

```text
Background="#FFF7EF" -> Background="{StaticResource AppBackgroundBrush}"
Background="#FFE1BB" -> Background="{StaticResource BoardBackgroundBrush}"
BorderBrush="#D79B63" -> BorderBrush="{StaticResource BoardBorderBrush}"
Background="#FFFFFB" -> Background="{StaticResource PanelBackgroundBrush}"
Foreground="#4A2D23" -> Foreground="{StaticResource TextPrimaryBrush}"
Foreground="#8A6655" -> Foreground="{StaticResource TextMutedBrush}"
Foreground="#D96B2B" -> Foreground="{StaticResource ScoreAccentBrush}"
Background="#7C4D2E" -> Background="{StaticResource AccentBrush}"
```

Leave piece-specific brushes from `CellViewModel` unchanged.

- [ ] **Step 2: Add settings panel under the New Game button**

Add this block below the New Game button:

```xml
<Border Background="{StaticResource PanelBackgroundBrush}"
        BorderBrush="#E8CFB7"
        BorderThickness="1"
        CornerRadius="10"
        Padding="18"
        Margin="0,14,0,0">
    <StackPanel>
        <TextBlock Text="Settings"
                   Foreground="{StaticResource TextMutedBrush}"
                   FontSize="13"
                   FontWeight="SemiBold" />
        <TextBlock Text="{Binding SelectedThemeName}"
                   Margin="0,8,0,0"
                   Foreground="{StaticResource TextPrimaryBrush}"
                   FontSize="15" />
        <TextBlock Text="{Binding AnimationIntensity, StringFormat=Animation: {0}}"
                   Margin="0,4,0,0"
                   Foreground="{StaticResource TextMutedBrush}"
                   FontSize="13" />
        <Button Content="Toggle Sound"
                Command="{Binding ToggleSoundCommand}"
                Height="34"
                Margin="0,12,0,0"
                Background="{StaticResource CellHoverBrush}"
                Foreground="{StaticResource TextPrimaryBrush}"
                BorderBrush="{StaticResource CellBorderBrush}" />
        <TextBlock Text="{Binding IsSoundEnabled, StringFormat=Sound enabled: {0}}"
                   Margin="0,6,0,0"
                   Foreground="{StaticResource TextMutedBrush}"
                   FontSize="12" />
    </StackPanel>
</Border>
```

- [ ] **Step 3: Update README theme section**

Append:

```markdown
## Theme Resources

The first theme is `CozyBoard`. Resource placeholders live under `src/ColorLines.Windows/Assets/Themes/CozyBoard/` with `board`, `pieces`, `effects`, and `sounds` folders.
```

- [ ] **Step 4: Build and test**

Run:

```powershell
dotnet test ColorLines.sln
dotnet build ColorLines.sln
```

Expected: all tests pass and build succeeds.

- [ ] **Step 5: Commit**

Run:

```powershell
git add src/ColorLines.Windows/MainWindow.xaml README.md
git commit -m "feat: bind ui to theme resources"
```

## Task 5: Phase 4 Verification

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: all tests pass.

- [ ] **Step 2: Build solution**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: build succeeds with zero errors.

- [ ] **Step 3: Launch WPF app**

Run:

```powershell
src\ColorLines.Windows\bin\Debug\net8.0-windows\ColorLines.Windows.exe
```

Expected: WPF app launches and shows the settings panel with `Cozy Board`, animation intensity, sound toggle, score, next cats, and board.

- [ ] **Step 4: Inspect git status**

Run:

```powershell
git status --short --branch
```

Expected: no uncommitted source changes.

## Self-Review

Spec coverage:

- Theme resource structure: Tasks 1, 3, and 4.
- Default `CozyBoard` theme: Tasks 1 and 3.
- Settings controls for theme, sound, and animation intensity: Tasks 2 and 4.
- Resource folder layout for board, pieces, effects, and sounds: Task 3.
- External asset loading and persistence are deferred to later phases.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

Type consistency:

- `ThemeInfo`, `ThemeCatalog`, `SelectedThemeName`, `IsSoundEnabled`, `AnimationIntensity`, and `ToggleSoundCommand` are consistently named across tests and UI binding.

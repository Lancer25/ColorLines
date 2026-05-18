# Color Lines Main Screen Visual Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the Windows main screen visual presentation while preserving current gameplay behavior and save data.

**Architecture:** Keep the work in the WPF presentation layer. `CozyBoard.xaml` owns reusable colors and brushes, `MainWindow.xaml` owns layout and named visual structure, and `WpfSmokeTests.cs` protects the named controls and important visual scaffolding.

**Tech Stack:** .NET 8, WPF XAML, xUnit smoke tests.

---

## File Structure

- Modify `src/ColorLines.Windows/Themes/CozyBoard.xaml`: refresh the cozy board palette and add reusable brushes for board depth, panels, cells, and buttons.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: name stable regions, use layered board styling, improve cell/piece presentation, and quiet the right rail.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: add smoke assertions for the named layout regions and visual controls.
- Modify `docs/superpowers/plans/2026-05-19-color-lines-main-screen-visual-upgrade.md`: mark tasks complete as they are finished.

---

## Task 1: Protect Main Screen Structure

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Add failing smoke assertions for named regions**

Update `OccupiedCellsShowPieceBody` in `tests/ColorLines.Tests/WpfSmokeTests.cs` by adding these lookups after `statusTextRegion`:

```csharp
var mainBoardFrame = FindVisualChildren<Border>(window)
    .First(border => border.Name == "MainBoardFrame");
var rightRail = FindVisualChildren<StackPanel>(window)
    .First(panel => panel.Name == "RightRail");
var newGameButton = FindVisualChildren<Button>(window)
    .First(button => button.Name == "NewGameButton");
```

Add these assertions near the other layout assertions:

```csharp
Assert.True(mainBoardFrame.Padding.Left >= 18);
Assert.True(rightRail.Margin.Left >= 24);
Assert.NotNull(newGameButton.Command);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: FAIL because `MainBoardFrame`, `RightRail`, or `NewGameButton` does not exist yet.

- [ ] **Step 3: Add stable XAML names**

In `src/ColorLines.Windows/MainWindow.xaml`, change the board container opening tag to include a name:

```xml
<Border x:Name="MainBoardFrame"
        Grid.Column="0"
        Background="{StaticResource BoardBackgroundBrush}"
        BorderBrush="{StaticResource BoardBorderBrush}"
        BorderThickness="2"
        CornerRadius="16"
        Padding="20">
```

Change the right rail stack panel opening tag:

```xml
<StackPanel x:Name="RightRail" Grid.Column="1" Margin="28,0,0,0">
```

Change the main New Game button:

```xml
<Button x:Name="NewGameButton"
        Content="New Game"
        Command="{Binding NewGameCommand}"
        Height="42"
        Background="{StaticResource AccentBrush}"
        Foreground="#FFFFFB"
        BorderThickness="0"
        FontSize="15"
        FontWeight="SemiBold" />
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add tests\ColorLines.Tests\WpfSmokeTests.cs src\ColorLines.Windows\MainWindow.xaml
git commit -m "test: protect main screen visual regions"
```

---

## Task 2: Refresh CozyBoard Theme Resources

**Files:**
- Modify: `src/ColorLines.Windows/Themes/CozyBoard.xaml`

- [ ] **Step 1: Add failing resource smoke test**

Add this test to `tests/ColorLines.Tests/WpfSmokeTests.cs`:

```csharp
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
    });
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: FAIL because the new brush keys do not exist.

- [ ] **Step 3: Add refreshed resources**

In `src/ColorLines.Windows/Themes/CozyBoard.xaml`, add these colors after the existing color keys:

```xml
<Color x:Key="BoardInnerColor">#FFF4D09C</Color>
<Color x:Key="BoardShadowColor">#4438150A</Color>
<Color x:Key="PanelBorderColor">#E7C8AC</Color>
<Color x:Key="CellInsetColor">#FFF2DDB8</Color>
<Color x:Key="PrimaryButtonHoverColor">#8F5834</Color>
```

Add these brushes after the existing brush keys:

```xml
<LinearGradientBrush x:Key="BoardInnerBrush" StartPoint="0,0" EndPoint="1,1">
    <GradientStop Color="#FFFFDFB2" Offset="0" />
    <GradientStop Color="{StaticResource BoardInnerColor}" Offset="0.52" />
    <GradientStop Color="#FFE7B06F" Offset="1" />
</LinearGradientBrush>
<SolidColorBrush x:Key="BoardShadowBrush" Color="{StaticResource BoardShadowColor}" />
<SolidColorBrush x:Key="PanelBorderBrush" Color="{StaticResource PanelBorderColor}" />
<SolidColorBrush x:Key="CellInsetBrush" Color="{StaticResource CellInsetColor}" />
<SolidColorBrush x:Key="PrimaryButtonHoverBrush" Color="{StaticResource PrimaryButtonHoverColor}" />
```

- [ ] **Step 4: Run resource test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter CozyBoardThemeExposesVisualUpgradeBrushes
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add tests\ColorLines.Tests\WpfSmokeTests.cs src\ColorLines.Windows\Themes\CozyBoard.xaml
git commit -m "feat: refresh cozy board theme resources"
```

---

## Task 3: Polish Board, Cells, And Pieces

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Add smoke assertions for piece base and calmer cell sizing**

In `OccupiedCellsShowPieceBody`, after the `pieceShadow` lookup, add:

```csharp
var pieceBase = FindVisualChildren<Ellipse>(occupiedButton)
    .First(ellipse => ellipse.Name == "PieceBase");
```

Add assertions:

```csharp
Assert.True(pieceBase.Width >= 42);
Assert.True(pieceBase.Opacity > 0);
Assert.True(pieceShadow.Width >= 38);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: FAIL because `PieceBase` does not exist yet.

- [ ] **Step 3: Update board and piece XAML**

In `MainWindow.xaml`, change `MainBoardFrame` to use a layered background:

```xml
<Border x:Name="MainBoardFrame"
        Grid.Column="0"
        Background="{StaticResource BoardShadowBrush}"
        BorderBrush="{StaticResource BoardBorderBrush}"
        BorderThickness="1"
        CornerRadius="18"
        Padding="6">
    <Border Background="{StaticResource BoardInnerBrush}"
            BorderBrush="#FFF8DCA8"
            BorderThickness="1"
            CornerRadius="14"
            Padding="18">
        <Viewbox Stretch="Uniform">
```

Close the extra inner `Border` before the closing `MainBoardFrame`.

Update the `BoardCellButton` style:

```xml
<Setter Property="Margin" Value="5" />
<Setter Property="Background" Value="{StaticResource CellBackgroundBrush}" />
<Setter Property="BorderBrush" Value="#D9D0A269" />
<Setter Property="BorderThickness" Value="1" />
```

Inside the cell grid, add a stable base ellipse directly before `PieceShadow`:

```xml
<Ellipse x:Name="PieceBase"
         Width="44"
         Height="23"
         VerticalAlignment="Bottom"
         Fill="#55FFF4DC"
         IsHitTestVisible="False">
    <Ellipse.Style>
        <Style TargetType="Ellipse">
            <Setter Property="Opacity" Value="0" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsOccupied}" Value="True">
                    <Setter Property="Opacity" Value="1" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Ellipse.Style>
</Ellipse>
```

Update `PieceShadow` size:

```xml
<Ellipse x:Name="PieceShadow"
         Width="40"
         Height="16"
         VerticalAlignment="Bottom"
         Fill="#3A24170F"
         IsHitTestVisible="False">
```

- [ ] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add tests\ColorLines.Tests\WpfSmokeTests.cs src\ColorLines.Windows\MainWindow.xaml
git commit -m "feat: polish board and piece presentation"
```

---

## Task 4: Polish Right Rail HUD

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Add smoke assertions for HUD names**

In `OccupiedCellsShowPieceBody`, add lookups:

```csharp
var scorePanel = FindVisualChildren<Border>(window)
    .First(border => border.Name == "ScorePanel");
var nextCatsPanel = FindVisualChildren<Border>(window)
    .First(border => border.Name == "NextCatsPanel");
var settingsPanel = FindVisualChildren<Border>(window)
    .First(border => border.Name == "SettingsPanel");
```

Add assertions:

```csharp
Assert.True(scorePanel.Padding.Left >= 16);
Assert.True(nextCatsPanel.Padding.Left >= 16);
Assert.True(settingsPanel.Padding.Left >= 16);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: FAIL because the panel names do not exist yet.

- [ ] **Step 3: Name and restyle HUD panels**

In `MainWindow.xaml`, name the score, next cats, and settings borders:

```xml
<Border x:Name="ScorePanel"
        Background="{StaticResource PanelBackgroundBrush}"
        BorderBrush="{StaticResource PanelBorderBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="18"
        Margin="0,0,0,14">
```

```xml
<Border x:Name="NextCatsPanel"
        Background="{StaticResource PanelBackgroundBrush}"
        BorderBrush="{StaticResource PanelBorderBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="18"
        Margin="0,0,0,14">
```

```xml
<Border x:Name="SettingsPanel"
        Background="{StaticResource PanelBackgroundBrush}"
        BorderBrush="{StaticResource PanelBorderBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="18"
        Margin="0,14,0,0">
```

Update `NewGameButton` to use a stronger height:

```xml
Height="44"
Background="{StaticResource AccentBrush}"
```

- [ ] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add tests\ColorLines.Tests\WpfSmokeTests.cs src\ColorLines.Windows\MainWindow.xaml
git commit -m "feat: polish right rail hud"
```

---

## Task 5: Full Verification

**Files:**
- Modify: `docs/superpowers/plans/2026-05-19-color-lines-main-screen-visual-upgrade.md`

- [ ] **Step 1: Run full tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: PASS.

- [ ] **Step 2: Run full build**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: PASS with 0 errors.

- [ ] **Step 3: Launch Windows app**

Run:

```powershell
Start-Process -FilePath (Resolve-Path 'src\ColorLines.Windows\bin\Debug\net8.0-windows\ColorLines.Windows.exe').Path -WorkingDirectory (Resolve-Path '.').Path -PassThru | Select-Object Id,ProcessName
```

Expected: the app launches and the default screen shows the upgraded board, right rail, cat pieces, and game-over overlay still covers the full window when triggered.

- [ ] **Step 4: Mark plan complete**

Replace every unchecked checkbox in this file with a checked checkbox.

- [ ] **Step 5: Commit**

```powershell
git add docs\superpowers\plans\2026-05-19-color-lines-main-screen-visual-upgrade.md
git commit -m "docs: mark main screen visual upgrade complete"
```

---

## Self-Review

- Spec coverage: board depth is covered by Task 3, calmer cells and piece base by Task 3, right rail HUD by Task 4, structural smoke tests by Tasks 1 through 4, full verification by Task 5.
- Placeholder scan: no TBD/TODO/fill-in markers remain.
- Type consistency: all referenced names are WPF names added in the same task before being relied on later.

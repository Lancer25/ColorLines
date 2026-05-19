# Color Lines Tabletop Main Menu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the plain main menu with a polished tabletop-style game title screen.

**Architecture:** Keep the existing shell commands and screen states. Update only WPF presentation and smoke tests: `MainMenuView` gets named hero, board preview, command zone, and status strip elements; `WpfSmokeTests` protects those named structures.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## File Structure

- Modify `src/ColorLines.Windows/MainWindow.xaml`: restyle `MainMenuView` into a tabletop composition.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: update main menu smoke coverage for hero board, command zone, status strip, and preview images.
- Modify `docs/superpowers/plans/2026-05-19-color-lines-tabletop-main-menu.md`: mark tasks complete.

---

## Task 1: Protect Tabletop Menu Structure

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Add failing tabletop smoke assertions**

In `MainMenuShowsActionsBeforeGameplay`, add these lookups after `menuSettingsButton`:

```csharp
var menuHeroBoard = FindVisualChildren<Border>(window)
    .First(border => border.Name == "MenuHeroBoard");
var menuCommandPanel = FindVisualChildren<Border>(window)
    .First(border => border.Name == "MenuCommandPanel");
var menuStatusStrip = FindVisualChildren<StackPanel>(window)
    .First(panel => panel.Name == "MenuStatusStrip");
var previewImages = FindVisualChildren<Image>(menuHeroBoard)
    .Where(image => image.Name.StartsWith("MenuPreviewCat", StringComparison.Ordinal))
    .ToArray();
```

Add these assertions:

```csharp
Assert.True(menuHeroBoard.Width >= 430);
Assert.True(menuCommandPanel.Padding.Left >= 24);
Assert.True(menuStatusStrip.Children.Count >= 3);
Assert.True(previewImages.Length >= 4);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because `MenuHeroBoard`, `MenuCommandPanel`, `MenuStatusStrip`, and named preview cats do not exist yet.

- [ ] **Step 3: Add stable names without changing layout yet**

In `MainWindow.xaml`:

- Add `x:Name="MenuHeroBoard"` and `Width="440"` to the existing menu preview `Border`.
- Add `x:Name="MenuPreviewCatOrange"` to the orange preview `Image`.
- Add `x:Name="MenuPreviewCatBlueGray"` to the blue-gray preview `Image`.
- Add `x:Name="MenuPreviewCatCalico"` to the calico preview `Image`.
- Add one more preview image in an empty cell named `MenuPreviewCatTuxedo` with source `/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/tuxedo.png`.
- Add `x:Name="MenuCommandPanel"` to the right-side command `Border`.
- Add a `StackPanel x:Name="MenuStatusStrip"` above `ContinueButton` containing three text rows:

```xml
<StackPanel x:Name="MenuStatusStrip">
    <TextBlock Text="{Binding Game.HighScore, StringFormat=Best Score: {0}}"
               FontSize="18"
               FontWeight="SemiBold"
               Foreground="{StaticResource TextPrimaryBrush}" />
    <TextBlock Text="{Binding Game.SelectedThemeName, StringFormat=Theme: {0}}"
               Margin="0,8,0,0"
               FontSize="13"
               Foreground="{StaticResource TextMutedBrush}" />
    <TextBlock Text="{Binding Game.AnimationIntensity, StringFormat=Animation: {0}}"
               Margin="0,4,0,0"
               FontSize="13"
               Foreground="{StaticResource TextMutedBrush}" />
</StackPanel>
```

Remove the old standalone `Best Score` text block so best score is represented only inside `MenuStatusStrip`.

- [ ] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "test: protect tabletop main menu structure"
```

---

## Task 2: Restyle Hero Board Zone

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Add failing hero-size assertion**

In `MainMenuShowsActionsBeforeGameplay`, add:

```csharp
Assert.True(menuHeroBoard.Height >= 430);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL if the hero board is still the old short 3x3 preview.

- [ ] **Step 3: Replace preview with a 5x5 tabletop board**

In `MainWindow.xaml`, replace the existing `MenuHeroBoard` content with:

```xml
<Border x:Name="MenuHeroBoard"
        Margin="0,30,0,0"
        Width="460"
        Height="460"
        Background="{StaticResource BoardShadowBrush}"
        BorderBrush="{StaticResource BoardBorderBrush}"
        BorderThickness="2"
        CornerRadius="20"
        Padding="16"
        HorizontalAlignment="Left">
    <Border Background="{StaticResource BoardInnerBrush}"
            CornerRadius="15"
            Padding="14">
        <UniformGrid Rows="5" Columns="5">
            <!-- 25 cells total; keep margins and heights stable -->
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatOrange"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/orange.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatBlueGray"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/bluegray.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatTuxedo"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/tuxedo.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatCalico"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/calico.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatWhite"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/white.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4">
                <Image x:Name="MenuPreviewCatGray"
                       Source="/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/gray.png"
                       Width="56"
                       Height="56"
                       Stretch="Uniform"
                       RenderOptions.BitmapScalingMode="HighQuality" />
            </Border>
            <Border Background="{StaticResource CellBackgroundBrush}" CornerRadius="10" Margin="4" />
        </UniformGrid>
    </Border>
</Border>
```

- [ ] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: enlarge tabletop menu board"
```

---

## Task 3: Polish Command Zone And Status Strip

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] **Step 1: Add command size assertions**

In `MainMenuShowsActionsBeforeGameplay`, add:

```csharp
Assert.True(continueButton.Height >= 52);
Assert.True(menuSettingsButton.Height >= 44);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: FAIL because the current menu buttons are smaller.

- [ ] **Step 3: Restyle command panel and buttons**

In `MainWindow.xaml`, update `MenuCommandPanel`:

```xml
<Border x:Name="MenuCommandPanel"
        Grid.Column="1"
        VerticalAlignment="Center"
        Background="{StaticResource PanelBackgroundBrush}"
        BorderBrush="{StaticResource PanelBorderBrush}"
        BorderThickness="1"
        CornerRadius="12"
        Padding="28"
        MinWidth="320">
```

Update button heights:

- `ContinueButton` height `54`, font size `16`, margin `0,26,0,0`.
- `MenuNewGameButton`, `MenuSettingsButton`, and `ExitButton` height `46`.

Add a small heading above `MenuStatusStrip`:

```xml
<TextBlock Text="Ready to play"
           FontSize="24"
           FontWeight="Bold"
           Foreground="{StaticResource TextPrimaryBrush}" />
```

- [ ] **Step 4: Run focused smoke test**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter MainMenuShowsActionsBeforeGameplay
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: polish tabletop menu commands"
```

---

## Task 4: Full Verification And Launch

**Files:**
- Modify: `docs/superpowers/plans/2026-05-19-color-lines-tabletop-main-menu.md`

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

Expected: the app launches to the redesigned tabletop main menu.

- [ ] **Step 4: Mark plan complete**

Replace every unchecked checkbox in this file with a checked checkbox.

- [ ] **Step 5: Commit**

```powershell
git add docs\superpowers\plans\2026-05-19-color-lines-tabletop-main-menu.md
git commit -m "docs: mark tabletop main menu complete"
```

---

## Self-Review

- Spec coverage: tabletop board is Task 2, command zone and status strip are Tasks 1 and 3, command bindings are protected in Task 1, full verification is Task 4.
- Placeholder scan: no placeholder markers remain.
- Type consistency: all named XAML elements used by tests are introduced in Task 1 before later visual refinements depend on them.

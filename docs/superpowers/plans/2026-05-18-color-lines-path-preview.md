# Color Lines Path Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Show reachable empty cells after selecting a cat, then show a path trail while hovering a reachable target.

**Architecture:** Keep movement rules in `ColorLines.Core.Rules.PathFinder`. Add derived preview flags and hover commands in `GameViewModel`, expose them through `CellViewModel`, and bind them to named WPF overlays in `MainWindow.xaml`.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM view model classes.

---

## File Structure

- Modify `src/ColorLines.Windows/ViewModels/CellViewModel.cs`: add `IsReachableTarget` and `IsPathPreview` record properties.
- Modify `src/ColorLines.Windows/ViewModels/GameViewModel.cs`: track preview path positions, compute reachable targets from selected cat, expose hover commands.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: add reachable and path overlay visuals and mouse enter/leave command bindings.
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`: add unit tests for reachable cells and hover path preview.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: verify the new named overlays exist in the board cell template.

---

### Task 1: Reachable Target Flags

**Files:**
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`

- [x] **Step 1: Write the failing test**

Add this test to `tests/ColorLines.Tests/GameViewModelTests.cs`:

```csharp
[Fact]
public void SelectingOccupiedCellMarksReachableEmptyTargets()
{
    var board = GameBoard.CreateEmpty();
    board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
    board.SetPiece(new BoardPosition(0, 1), PieceKind.Gray);
    board.SetPiece(new BoardPosition(1, 0), PieceKind.Gray);
    var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
    var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
    var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);

    viewModel.SelectCellCommand.Execute(source);

    Assert.Contains(viewModel.Cells, cell => cell.Row == 1 && cell.Column == 1 && cell.IsReachableTarget);
    Assert.DoesNotContain(viewModel.Cells, cell => cell.IsOccupied && cell.IsReachableTarget);
}
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter SelectingOccupiedCellMarksReachableEmptyTargets
```

Expected: FAIL because `CellViewModel` does not expose `IsReachableTarget`.

- [x] **Step 3: Add preview flags to `CellViewModel`**

Change the record signature in `src/ColorLines.Windows/ViewModels/CellViewModel.cs` to include:

```csharp
bool WasRejectedTarget,
bool IsReachableTarget,
bool IsPathPreview)
```

Update `Empty` to accept:

```csharp
bool isReachableTarget = false,
bool isPathPreview = false
```

and pass the two values into the record constructor.

Update `Occupied` to accept:

```csharp
bool isPathPreview = false
```

and pass `false` for `IsReachableTarget` plus `isPathPreview` for `IsPathPreview`.

- [x] **Step 4: Compute reachable targets in `GameViewModel`**

Add this helper in `src/ColorLines.Windows/ViewModels/GameViewModel.cs`:

```csharp
private bool IsReachableTarget(BoardPosition position)
{
    return selectedPosition is not null
        && state.Board.GetPiece(position) is null
        && PathFinder.FindPath(state.Board, selectedPosition.Value, position).Count > 0;
}
```

In `RefreshFromState`, before constructing the cell view model, compute:

```csharp
var isReachableTarget = IsReachableTarget(cell.Position);
```

Pass `isReachableTarget` into `CellViewModel.Empty(...)`. Occupied cells should always pass no reachable flag.

- [x] **Step 5: Run test to verify it passes**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter SelectingOccupiedCellMarksReachableEmptyTargets
```

Expected: PASS.

- [x] **Step 6: Commit**

Run:

```powershell
git add src\ColorLines.Windows\ViewModels\CellViewModel.cs src\ColorLines.Windows\ViewModels\GameViewModel.cs tests\ColorLines.Tests\GameViewModelTests.cs
git commit -m "feat: mark reachable move targets"
```

---

### Task 2: Hover Path Preview Commands

**Files:**
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`

- [x] **Step 1: Write the failing tests**

Add these tests to `tests/ColorLines.Tests/GameViewModelTests.cs`:

```csharp
[Fact]
public void PreviewPathCommandMarksPathCellsForReachableTarget()
{
    var board = GameBoard.CreateEmpty();
    board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
    var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
    var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
    var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
    viewModel.SelectCellCommand.Execute(source);
    var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);

    viewModel.PreviewPathCommand.Execute(target);

    Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 0 && cell.IsPathPreview);
    Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsPathPreview);
    Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 2 && cell.IsPathPreview);
}

[Fact]
public void ClearPreviewPathCommandRemovesPathWithoutClearingSelection()
{
    var board = GameBoard.CreateEmpty();
    board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
    var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
    var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
    var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
    viewModel.SelectCellCommand.Execute(source);
    var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);
    viewModel.PreviewPathCommand.Execute(target);

    viewModel.ClearPreviewPathCommand.Execute(null);

    Assert.DoesNotContain(viewModel.Cells, cell => cell.IsPathPreview);
    Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 0 && cell.IsSelected);
}
```

- [x] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "PreviewPathCommandMarksPathCellsForReachableTarget|ClearPreviewPathCommandRemovesPathWithoutClearingSelection"
```

Expected: FAIL because `PreviewPathCommand` and `ClearPreviewPathCommand` do not exist.

- [x] **Step 3: Add preview command state**

In `src/ColorLines.Windows/ViewModels/GameViewModel.cs`, add a field:

```csharp
private HashSet<BoardPosition> pathPreviewPositions;
```

Initialize it in the constructor:

```csharp
pathPreviewPositions = new HashSet<BoardPosition>();
```

Expose commands:

```csharp
public ICommand PreviewPathCommand { get; }

public ICommand ClearPreviewPathCommand { get; }
```

Initialize them in the constructor:

```csharp
PreviewPathCommand = new RelayCommand(PreviewPath, parameter => parameter is CellViewModel);
ClearPreviewPathCommand = new RelayCommand(_ => ClearPreviewPath());
```

- [x] **Step 4: Implement preview methods**

Add these methods in `src/ColorLines.Windows/ViewModels/GameViewModel.cs`:

```csharp
private void PreviewPath(object? parameter)
{
    pathPreviewPositions.Clear();

    if (selectedPosition is null || parameter is not CellViewModel cell || cell.IsOccupied)
    {
        RefreshFromState();
        return;
    }

    var target = new BoardPosition(cell.Row, cell.Column);
    var path = PathFinder.FindPath(state.Board, selectedPosition.Value, target);
    if (path.Count > 0)
    {
        pathPreviewPositions.UnionWith(path);
    }

    RefreshFromState();
}

private void ClearPreviewPath()
{
    if (pathPreviewPositions.Count == 0)
    {
        return;
    }

    pathPreviewPositions.Clear();
    RefreshFromState();
}
```

Call `pathPreviewPositions.Clear();` inside `SelectCell` when selecting a different occupied piece, after rejected empty clicks, after successful moves, and inside `NewGame`.

In `RefreshFromState`, compute:

```csharp
var isPathPreview = pathPreviewPositions.Contains(cell.Position);
```

Pass `isPathPreview` into both empty and occupied cell constructors.

- [x] **Step 5: Run tests to verify they pass**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "PreviewPathCommandMarksPathCellsForReachableTarget|ClearPreviewPathCommandRemovesPathWithoutClearingSelection"
```

Expected: PASS.

- [x] **Step 6: Commit**

Run:

```powershell
git add src\ColorLines.Windows\ViewModels\CellViewModel.cs src\ColorLines.Windows\ViewModels\GameViewModel.cs tests\ColorLines.Tests\GameViewModelTests.cs
git commit -m "feat: preview reachable cat paths"
```

---

### Task 3: WPF Board Overlays

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `src/ColorLines.Windows/MainWindow.xaml.cs`

- [x] **Step 1: Write the failing WPF smoke test assertions**

In `tests/ColorLines.Tests/WpfSmokeTests.cs`, inside `OccupiedCellsShowPieceBody`, add assertions that named overlays exist:

```csharp
var reachableTargetGlow = FindVisualChildren<Border>(occupiedButton)
    .First(border => border.Name == "ReachableTargetGlow");
var pathPreviewGlow = FindVisualChildren<Border>(occupiedButton)
    .First(border => border.Name == "PathPreviewGlow");

Assert.Equal(0, reachableTargetGlow.Opacity);
Assert.Equal(0, pathPreviewGlow.Opacity);
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: FAIL because `ReachableTargetGlow` and `PathPreviewGlow` do not exist.

- [x] **Step 3: Add board overlays**

In `src/ColorLines.Windows/MainWindow.xaml`, add hover handlers to the board cell `Button`:

```xml
MouseEnter="BoardCellPointerEntered"
MouseLeave="BoardCellPointerLeft"
```

Add these overlays near the existing feedback glows, below `PieceActor`:

```xml
<Border x:Name="ReachableTargetGlow"
        Margin="8"
        CornerRadius="16"
        Background="#FFFFE1A8"
        Opacity="0"
        IsHitTestVisible="False">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Opacity" Value="0" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsReachableTarget}" Value="True">
                    <Setter Property="Opacity" Value="0.42" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
</Border>

<Border x:Name="PathPreviewGlow"
        Margin="14"
        CornerRadius="14"
        Background="#FF7AC7C4"
        Opacity="0"
        IsHitTestVisible="False">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Opacity" Value="0" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsPathPreview}" Value="True">
                    <Setter Property="Opacity" Value="0.55" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
</Border>
```

- [x] **Step 4: Wire hover forwarding without adding dependencies**

Add `using System.Windows.Input;` to `src/ColorLines.Windows/MainWindow.xaml.cs`, then add handlers inside `MainWindow`:

```csharp
private void BoardCellPointerEntered(object sender, MouseEventArgs e)
{
    if (DataContext is GameViewModel viewModel && sender is FrameworkElement element)
    {
        viewModel.PreviewPathCommand.Execute(element.DataContext);
    }
}

private void BoardCellPointerLeft(object sender, MouseEventArgs e)
{
    if (DataContext is GameViewModel viewModel)
    {
        viewModel.ClearPreviewPathCommand.Execute(null);
    }
}
```

- [x] **Step 5: Run WPF smoke tests**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests
```

Expected: PASS.

- [x] **Step 6: Commit**

Run:

```powershell
git add src\ColorLines.Windows\MainWindow.xaml src\ColorLines.Windows\MainWindow.xaml.cs tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: add path preview overlays"
```

---

### Task 4: Full Verification

**Files:**
- Modify: `docs/superpowers/plans/2026-05-18-color-lines-path-preview.md`

- [x] **Step 1: Run full tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: PASS with all tests green.

- [x] **Step 2: Run full build**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: successful build with 0 errors.

- [x] **Step 3: Launch the WPF app**

Run:

```powershell
Start-Process -FilePath (Resolve-Path 'src\ColorLines.Windows\bin\Debug\net8.0-windows\ColorLines.Windows.exe').Path -WorkingDirectory (Resolve-Path '.').Path -PassThru | Select-Object Id,ProcessName
```

Expected: the app window opens and selecting a cat shows reachable cells. Hovering a reachable cell shows a path trail.

- [x] **Step 4: Mark this plan complete**

Update this plan's checkboxes to completed after verification.

- [x] **Step 5: Commit completion marker**

Run:

```powershell
git add docs\superpowers\plans\2026-05-18-color-lines-path-preview.md
git commit -m "docs: mark path preview complete"
```

---

## Self-Review

- Spec coverage: reachable targets, hover path preview, WPF overlays, and verification are covered by Tasks 1-4.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: `IsReachableTarget`, `IsPathPreview`, `PreviewPathCommand`, and `ClearPreviewPathCommand` are used consistently across tests, view models, and WPF bindings.

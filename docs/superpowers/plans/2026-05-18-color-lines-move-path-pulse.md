# Color Lines Move Path Pulse Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Pulse the cells in the completed movement path after a successful cat move.

**Architecture:** Reuse `GameEventKind.PieceMoved` path positions. Store them in `GameViewModel`, expose them through `CellViewModel.WasMovePath`, and bind a WPF overlay animation to that flag.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM classes.

---

## File Structure

- Modify `src/ColorLines.Windows/ViewModels/CellViewModel.cs`: add `WasMovePath`.
- Modify `src/ColorLines.Windows/ViewModels/GameViewModel.cs`: track movement path feedback separately from destination feedback.
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`: add unit tests for move path feedback.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: add `MovePathPulseGlow`.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: verify the named overlay exists.

---

### Task 1: Move Path Feedback State

**Files:**
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`

- [x] **Step 1: Write failing tests**

Add tests proving a successful move marks the whole path and that selecting another cat clears old path feedback.

- [x] **Step 2: Verify tests fail**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "SuccessfulMoveMarksPathCells|SelectingOccupiedCellClearsMovePathFeedback"
```

Expected: FAIL because `WasMovePath` does not exist.

- [x] **Step 3: Implement `WasMovePath`**

Add the property to `CellViewModel`, track `movePathPositions` in `GameViewModel`, fill it from `GameEventKind.PieceMoved`, clear it via `ClearCellFeedback`, and pass the flag through `RefreshFromState`.

- [x] **Step 4: Verify tests pass**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter "SuccessfulMoveMarksPathCells|SelectingOccupiedCellClearsMovePathFeedback"
```

Expected: PASS.

- [x] **Step 5: Commit**

Run:

```powershell
git add src\ColorLines.Windows\ViewModels\CellViewModel.cs src\ColorLines.Windows\ViewModels\GameViewModel.cs tests\ColorLines.Tests\GameViewModelTests.cs
git commit -m "feat: mark completed move paths"
```

---

### Task 2: WPF Move Path Pulse

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [x] **Step 1: Write failing smoke assertion**

Assert that `MovePathPulseGlow` exists and is hidden by default.

- [x] **Step 2: Verify test fails**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody
```

Expected: FAIL because `MovePathPulseGlow` does not exist.

- [x] **Step 3: Add WPF overlay**

Add a non-hit-testable overlay named `MovePathPulseGlow`, below the cat image layer, with opacity animation bound to `WasMovePath`.

- [x] **Step 4: Verify WPF smoke tests pass**

Run:

```powershell
dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests
```

Expected: PASS.

- [x] **Step 5: Commit**

Run:

```powershell
git add src\ColorLines.Windows\MainWindow.xaml tests\ColorLines.Tests\WpfSmokeTests.cs
git commit -m "feat: pulse completed move paths"
```

---

### Task 3: Full Verification

**Files:**
- Modify: `docs/superpowers/plans/2026-05-18-color-lines-move-path-pulse.md`

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

- [x] **Step 3: Launch app**

Run:

```powershell
Start-Process -FilePath (Resolve-Path 'src\ColorLines.Windows\bin\Debug\net8.0-windows\ColorLines.Windows.exe').Path -WorkingDirectory (Resolve-Path '.').Path -PassThru | Select-Object Id,ProcessName
```

Expected: app starts, selecting a cat and moving it shows a brief path pulse.

- [x] **Step 4: Mark plan complete and commit**

Check all boxes in this file and commit with:

```powershell
git add docs\superpowers\plans\2026-05-18-color-lines-move-path-pulse.md
git commit -m "docs: mark move path pulse complete"
```

---

## Self-Review

- Spec coverage: movement path state, WPF overlay, and verification are covered.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: `WasMovePath` and `MovePathPulseGlow` are used consistently.

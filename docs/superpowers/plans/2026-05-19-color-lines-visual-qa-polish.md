# Color Lines Visual QA Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Polish the game-over modal and close the end-of-run navigation loop.

**Architecture:** Keep the existing WPF `GameOverOverlay` and shell commands. Add smoke-test protection for named modal regions and command bindings, then restyle the modal in `MainWindow.xaml` using existing button styles.

**Tech Stack:** .NET 8, WPF XAML, xUnit WPF smoke tests.

---

## Task 1: Protect Game-Over Actions

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Add `GameOverDialog`, `GameOverActionBar`, `GameOverNewGameButton`, and `GameOverMenuButton` lookups in `OccupiedCellsShowPieceBody`.
- [ ] Assert action bar count, exact command bindings, and button style tags.
- [ ] Run `dotnet test ColorLines.sln --filter OccupiedCellsShowPieceBody` and confirm it fails before XAML implementation.
- [ ] Commit with `git commit -m "test: protect game over action layout"`.

## Task 2: Restyle Game-Over Modal

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Name the dialog `GameOverDialog`.
- [ ] Use existing panel/settings/gameplay brushes for the dialog and score block.
- [ ] Replace the raw New Game button with `GameOverNewGameButton` using `MenuPrimaryButton`.
- [ ] Add `GameOverMenuButton` using `GameplayCompactButton` and `BackToMenuCommand`.
- [ ] Wrap both buttons in `GameOverActionBar`.
- [ ] Run `dotnet test ColorLines.sln --filter OccupiedCellsShowPieceBody` and confirm it passes.
- [ ] Commit with `git commit -m "feat: polish game over actions"`.

## Task 3: Full Verification

**Files:**
- No planned source modifications unless verification exposes a bug.

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app with `Start-Process -FilePath 'dotnet' -ArgumentList 'run --project src/ColorLines.Windows/ColorLines.Windows.csproj' -WorkingDirectory (Resolve-Path '.').Path -WindowStyle Hidden -PassThru | Select-Object Id,ProcessName`.

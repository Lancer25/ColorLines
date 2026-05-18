# Color Lines Piece Fit Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tune PNG cat piece sizing, shadows, and selected-piece treatment so pieces feel seated in board cells.

**Architecture:** Keep game logic and assets unchanged. Use WPF template changes plus smoke tests that verify named visual layers and dimensions.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Task 1: Add Visual Fit Smoke Coverage

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Add assertions for a named `PieceShadow` and polished `PieceImage` dimensions.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody`; expect failure because `PieceShadow` is not named yet or dimensions still use the old value.
- [x] Commit only after Task 2 passes.

## Task 2: Polish Board And Preview Piece Fit

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Name the occupied-piece shadow `PieceShadow`.
- [x] Reduce board `PieceImage` from 48x48 to 44x44 and center it in a 52x52 cell content area.
- [x] Adjust soft feedback glows to sit behind the smaller image.
- [x] Adjust next-piece preview image size to match the new proportions.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect pass.
- [x] Commit with `feat: polish cat piece fit`.

## Task 3: Verify And Launch

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app.
- [x] Confirm the process responds.
- [x] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Board piece size and padding: Task 2.
- Ground shadow: Task 1 and Task 2.
- Next-piece preview proportion: Task 2.
- No game logic changes: all tasks are WPF/test only.

Placeholder scan:

- The plan contains no unresolved placeholder markers.

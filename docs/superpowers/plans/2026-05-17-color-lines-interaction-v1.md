# Color Lines Interaction V1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add lightweight visual feedback for move targets, spawned pieces, cleared pieces, and invalid moves.

**Architecture:** Keep core rules unchanged. `GameViewModel` converts `GameEvent` data into short-lived cell presentation flags, and WPF styles render those flags as glow rings and status feedback.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Scope

- Add cell flags: `WasMovedTo`, `WasSpawned`, `WasCleared`, and `WasRejectedTarget`.
- Populate flags from the latest `GameTurnResult`.
- Add tests proving flags are surfaced after moves and rejected clicks.
- Update WPF XAML to show move/spawn/clear/rejected rings.
- Verify tests, build, and WPF launch.

## Tasks

### Task 1: Add Cell Feedback Flags

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [x] Add failing tests for rejected target feedback and move target feedback.
- [x] Add feedback flags to `CellViewModel`.
- [x] Store latest moved/spawned/cleared/rejected positions in `GameViewModel`.
- [x] Refresh cells with those flags.
- [x] Run view model tests.
- [x] Commit with `feat: add cell feedback flags`.

### Task 2: Add WPF Feedback Rings

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Add layered borders for moved, spawned, cleared, and rejected cells.
- [x] Add a smoke test that a highlighted moved or selected visual can be created.
- [x] Run WPF smoke tests.
- [ ] Commit with `feat: add board feedback rings`.

### Task 3: Verify And Launch

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch WPF exe.
- [ ] Confirm process responds.
- [ ] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Selected/move feedback: Task 1 and Task 2.
- Invalid move feedback: Task 1 and Task 2.
- Spawn/clear visual markers: Task 1 and Task 2.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

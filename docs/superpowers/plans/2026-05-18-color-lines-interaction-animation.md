# Color Lines Interaction Animation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add local WPF animation structure and lightweight feedback animations for selecting, moving, spawning, and rejected clicks.

**Architecture:** Keep game logic unchanged. Wrap the board piece image in a named transform container and use `DataTrigger.EnterActions` storyboards bound to existing cell flags.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Task 1: Add Animation Structure Test

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Add WPF smoke assertions for named `PieceActor` and a `TransformGroup` with scale and translate transforms.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter OccupiedCellsShowPieceBody`; expect failure because `PieceActor` does not exist yet.
- [x] Commit only after Task 2 passes.

## Task 2: Add Piece Actor And Local Animations

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Wrap `PieceImage` in `Grid x:Name="PieceActor"`.
- [x] Add `TransformGroup` with `ScaleTransform` and `TranslateTransform` to `PieceActor`.
- [x] Add selected lift animation using `IsSelected`.
- [x] Add pop animations for `WasMovedTo` and `WasSpawned`.
- [x] Add subtle rejected target feedback animation on `RejectFeedbackGlow`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect pass.
- [x] Commit with `feat: add board interaction animations`.

## Task 3: Verify And Launch

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app.
- [x] Confirm the process responds.
- [x] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Selected lift: Task 2.
- Move/spawn pop: Task 2.
- Rejected feedback: Task 2.
- Future path movement structure: Task 1 and Task 2.
- No rule/save changes: all tasks are XAML/test only.

Placeholder scan:

- The plan contains no unresolved placeholder markers.

# Color Lines Animation Feedback Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the existing `Animation: Full/Reduced` setting affect board and score feedback animations.

**Architecture:** Keep all gameplay rules and view-model state unchanged. Update WPF smoke tests to require XAML bindings to `Game.IsFullAnimation`, then modify transient feedback styles in `MainWindow.xaml` so Full mode runs storyboards and Reduced mode shows low-opacity static cues.

**Tech Stack:** .NET 8, WPF XAML, xUnit WPF smoke tests.

---

## File Structure

- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: add XML-level tests for reduced-animation wiring.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: update feedback layer triggers.
- Verify with `dotnet test ColorLines.sln` and `dotnet build ColorLines.sln`.

## Task 1: Protect Reduced Animation Wiring

**Files:**
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Add a test named `TransientFeedbackLayersRespectAnimationIntensity`.
- [ ] In the test, load `MainWindow.xaml` with `XDocument`.
- [ ] For each layer name `MovePathPulseGlow`, `ClearFeedbackGlow`, `ClearPulseGlow`, `RejectFeedbackGlow`, `SpawnFeedbackGlow`, `MoveFeedbackGlow`, and `ScoreDeltaBadge`, assert that its descendants contain text or attributes referencing `Game.IsFullAnimation`.
- [ ] Assert that the `PieceScaleActor` descendants reference `Game.IsFullAnimation`.
- [ ] Run `dotnet test ColorLines.sln --filter TransientFeedbackLayersRespectAnimationIntensity` and confirm it fails before XAML implementation.
- [ ] Commit with `git commit -m "test: protect reduced animation feedback wiring"`.

## Task 2: Wire Feedback Layers to Animation Intensity

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Replace each one-condition transient `DataTrigger` with `MultiDataTrigger` branches:
  - Full branch: original feedback flag is true and `Game.IsFullAnimation` is true, with the existing storyboard.
  - Reduced branch: original feedback flag is true and `Game.IsFullAnimation` is false, with a static low opacity setter.
- [ ] Apply this pattern to `MovePathPulseGlow`, `ClearFeedbackGlow`, `ClearPulseGlow`, `RejectFeedbackGlow`, `SpawnFeedbackGlow`, `MoveFeedbackGlow`, and `ScoreDeltaBadge`.
- [ ] For `PieceScaleActor`, gate the existing `WasMovedTo` and `WasSpawned` opacity storyboards behind `Game.IsFullAnimation == True`.
- [ ] Keep `ReachableTargetGlow`, `PathPreviewGlow`, selected-piece base/shadow, and occupied-piece opacity unchanged.
- [ ] Run `dotnet test ColorLines.sln --filter TransientFeedbackLayersRespectAnimationIntensity` and confirm it passes.
- [ ] Run `dotnet test ColorLines.sln --filter OccupiedCellsShowPieceBody` and confirm it passes.
- [ ] Commit with `git commit -m "feat: respect reduced animation feedback"`.

## Task 3: Full Verification and Launch

**Files:**
- No planned source modifications unless verification exposes a bug.

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app with `Start-Process -FilePath 'dotnet' -ArgumentList 'run --project src/ColorLines.Windows/ColorLines.Windows.csproj' -WorkingDirectory (Resolve-Path '.').Path -WindowStyle Hidden -PassThru | Select-Object Id,ProcessName`.

# Color Lines Animation Contrast Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Full and Reduced animation modes feel visibly different.

**Architecture:** Keep existing view-model state and WPF triggers. Add an XML smoke test that caps reduced-mode opacity, then adjust `MainWindow.xaml` feedback trigger values.

**Tech Stack:** .NET 8, WPF XAML, xUnit WPF smoke tests.

---

## Task 1: Protect Reduced Mode Calmness

**Files:**
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Add a smoke test that inspects reduced-mode `MultiDataTrigger` setters for `MovePathPulseGlow`, `ClearFeedbackGlow`, `ClearPulseGlow`, `RejectFeedbackGlow`, `SpawnFeedbackGlow`, `MoveFeedbackGlow`, and `ScoreDeltaBadge`.
- [ ] Assert reduced-mode opacity setters are `0` or not greater than `0.08`.
- [ ] Run `dotnet test ColorLines.sln --filter ReducedAnimationKeepsTransientFeedbackSubtle` and confirm it fails before implementation.
- [ ] Commit with `git commit -m "test: protect reduced animation calmness"`.

## Task 2: Increase Mode Contrast

**Files:**
- Modify `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Set reduced-mode transient feedback opacity setters to `0`.
- [ ] Strengthen full-mode fade `From` values enough to be more visible.
- [ ] Keep path preview, reachable targets, selection, piece base, and piece shadow unchanged.
- [ ] Run `dotnet test ColorLines.sln --filter "ReducedAnimationKeepsTransientFeedbackSubtle|TransientFeedbackLayersRespectAnimationIntensity|OccupiedCellsShowPieceBody"`.
- [ ] Commit with `git commit -m "feat: increase animation mode contrast"`.

## Task 3: Verify

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app for playtest.

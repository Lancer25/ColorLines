# Color Lines Soft Visual Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace harsh feedback rings and symbol-like cats with softer WPF-native game visuals.

**Architecture:** Keep game logic unchanged. Extend `PieceViewModel` with brushes used by a richer cat avatar template, then update `MainWindow.xaml` to render soft cell glows and shape-based cat faces.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## File Structure

- Modify `src/ColorLines.Windows/ViewModels/PieceViewModel.cs`: add visual brushes for highlight, shadow, and inner ear accents.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: replace hard feedback borders and text cat faces with soft glow layers and shape-based cat avatars.
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`: verify the new visual brushes are exposed.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: verify upgraded visual elements exist and old feedback rings are no longer required.

## Task 1: Extend Piece Presentation

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/PieceViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [x] Add a failing test named `PieceViewModelProvidesAvatarAccentBrushes`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter PieceViewModelProvidesAvatarAccentBrushes`; expect compile failure for missing properties.
- [x] Add `HighlightBrush`, `ShadowBrush`, and `InnerEarBrush` to `PieceViewModel`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter GameViewModelTests`; expect pass.
- [ ] Commit with `feat: add cat avatar accent brushes`.

## Task 2: Replace Hard Rings With Soft Visuals

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Add a failing WPF smoke assertion for `CatFaceLayer` and `MoveFeedbackGlow`.
- [ ] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect failure because the new named elements do not exist.
- [ ] Replace the cell feedback borders with soft `Ellipse`/`Border` glow layers named `MoveFeedbackGlow`, `SpawnFeedbackGlow`, `ClearFeedbackGlow`, and `RejectFeedbackGlow`.
- [ ] Replace text-based cat faces with shape-based face elements inside a named `CatFaceLayer`.
- [ ] Update next-cat previews to use the same smaller avatar style.
- [ ] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect pass.
- [ ] Commit with `feat: soften board visuals`.

## Task 3: Verify And Launch

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the WPF app.
- [ ] Confirm the process responds.
- [ ] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Soft move/spawn/clear/reject feedback: Task 2.
- Shape-based cat avatars: Task 1 and Task 2.
- Next-cat preview consistency: Task 2.
- No game logic changes: all tasks are WPF/ViewModel presentation only.

Placeholder scan:

- The plan contains no unresolved placeholder markers.

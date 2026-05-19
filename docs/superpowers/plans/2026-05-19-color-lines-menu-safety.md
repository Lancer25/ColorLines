# Color Lines Menu Safety Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove unsafe new-game access from the main menu and require confirmation before returning from gameplay to the main menu.

**Architecture:** Keep the behavior in `ShellViewModel` because it controls shell navigation. Update WPF bindings so the pause menu requests confirmation first, while game-over can still return directly to the main menu.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## Task 1: Protect Shell Confirmation Behavior

**Files:**
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Add `PauseMenuReturnToMainMenuRequiresConfirmation`.
- [ ] The test creates a `ShellViewModel`, enters playing, opens pause menu, executes `RequestBackToMenuCommand`, and expects pause menu to remain visible while `IsReturnToMenuConfirmVisible` is true.
- [ ] The test then executes `CancelBackToMenuCommand` and expects pause menu to remain visible while confirmation is false.
- [ ] The test requests again, executes `ConfirmBackToMenuCommand`, and expects main menu visible.
- [ ] Run `dotnet test ColorLines.sln --filter PauseMenuReturnToMainMenuRequiresConfirmation`.
- [ ] Commit with `git commit -m "test: protect return menu confirmation"`.

## Task 2: Implement Shell Confirmation Commands

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`

- [ ] Add `IsReturnToMenuConfirmVisible`.
- [ ] Add `RequestBackToMenuCommand`, `ConfirmBackToMenuCommand`, and `CancelBackToMenuCommand`.
- [ ] Reset confirmation state when continuing, starting a new game, opening settings, saving, returning to game, or returning to main menu.
- [ ] Keep existing `BackToMenuCommand` for safe direct contexts.
- [ ] Run `dotnet test ColorLines.sln --filter PauseMenuReturnToMainMenuRequiresConfirmation`.
- [ ] Commit with `git commit -m "feat: require confirmation before menu return"`.

## Task 3: Protect WPF Menu Layout

**Files:**
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Update main menu smoke test to assert `MenuNewGameButton` is absent.
- [ ] Update gameplay smoke test to assert `PauseBackToMenuButton` binds to `RequestBackToMenuCommand`.
- [ ] Add assertions for `ReturnToMenuConfirmPanel`, `ReturnToMenuCancelButton`, and `ReturnToMenuConfirmButton`.
- [ ] Run `dotnet test ColorLines.sln --filter "MainMenuShowsActionsBeforeGameplay|OccupiedCellsShowPieceBody"`.
- [ ] Commit with `git commit -m "test: protect menu safety layout"`.

## Task 4: Update WPF Menu UI

**Files:**
- Modify `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Remove the main menu `MenuNewGameButton`.
- [ ] Bind `PauseBackToMenuButton` to `RequestBackToMenuCommand`.
- [ ] Add `ReturnToMenuConfirmPanel` under the pause menu actions.
- [ ] Add warning text and two buttons: `ReturnToMenuCancelButton` and `ReturnToMenuConfirmButton`.
- [ ] Run `dotnet test ColorLines.sln --filter "MainMenuShowsActionsBeforeGameplay|OccupiedCellsShowPieceBody"`.
- [ ] Commit with `git commit -m "feat: add return menu confirmation ui"`.

## Task 5: Full Verification

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the WPF app for manual playtest.

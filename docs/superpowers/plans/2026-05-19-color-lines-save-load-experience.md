# Color Lines Save Load Experience Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the existing local save/load behavior visible to the player without changing the save file format.

**Architecture:** Add derived save summary text to the shell view model, show it in the main menu status strip, and keep pause menu save feedback visible. Use existing save service and commands.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## Task 1: Protect Save Summary ViewModel Text

**Files:**
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Add a test for `ShellViewModel.SaveSummaryText` on a new game.
- [ ] Add a test that score/high score changes appear in `SaveSummaryText`.
- [ ] Add a test that `SaveGameCommand` updates `PauseSaveStatusText`.
- [ ] Run targeted tests and confirm failures before implementation.
- [ ] Commit with `git commit -m "test: protect save summary text"`.

## Task 2: Implement Save Summary Text

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`

- [ ] Add `SaveSummaryText`.
- [ ] Subscribe to relevant `Game.PropertyChanged` notifications and raise `SaveSummaryText` updates when score/high score changes.
- [ ] Keep `PauseSaveStatusText` update behavior.
- [ ] Run targeted tests and confirm pass.
- [ ] Commit with `git commit -m "feat: add save summary text"`.

## Task 3: Show Save Summary on Main Menu

**Files:**
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`
- Modify `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Add WPF smoke assertion for `MenuSaveSummary`.
- [ ] Add `MenuSaveSummary` text block bound to `SaveSummaryText` in `MenuStatusStrip`.
- [ ] Run `dotnet test ColorLines.sln --filter MainMenuShowsActionsBeforeGameplay`.
- [ ] Commit with `git commit -m "feat: show save summary on main menu"`.

## Task 4: Full Verification and Launch

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app for manual playtest.

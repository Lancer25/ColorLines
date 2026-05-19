# Color Lines Pause Menu Navigation Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace direct gameplay actions with a game-style pause menu and make settings return to the correct source screen.

**Architecture:** Add `PauseMenu` to the shell state model, add pause/settings commands to `ShellViewModel`, update WPF tests, and add a `PauseMenuView` in `MainWindow.xaml`. Keep gameplay rules, save data format, game-over actions, and performance behavior unchanged.

**Tech Stack:** .NET 8, WPF XAML, xUnit.

---

## Task 1: Protect Shell Navigation Model

**Files:**
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Update `SettingsCommandOpensSettingsScreenAndBackReturnsToMenu` to execute `CloseSettingsCommand` instead of `BackToMenuCommand`.
- [ ] Add `PauseMenuCommandOpensPauseMenuAndContinueReturnsToGame`.
- [ ] Add `SettingsOpenedFromPauseMenuReturnsToPauseMenu`.
- [ ] Add `SaveGameCommandRaisesSaveRequestedAndUpdatesStatus`.
- [ ] Run `dotnet test ColorLines.sln --filter "PauseMenuCommandOpensPauseMenuAndContinueReturnsToGame|SettingsOpenedFromPauseMenuReturnsToPauseMenu|SaveGameCommandRaisesSaveRequestedAndUpdatesStatus|SettingsCommandOpensSettingsScreenAndBackReturnsToMenu"` and confirm failures before implementation.
- [ ] Commit with `git commit -m "test: protect pause menu shell navigation"`.

## Task 2: Implement Shell Pause Menu State

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/ShellScreen.cs`
- Modify `src/ColorLines.Windows/ViewModels/ShellViewModel.cs`
- Modify `src/ColorLines.Windows/MainWindow.xaml.cs`

- [ ] Add `PauseMenu` to `ShellScreen`.
- [ ] Add `IsPauseMenuVisible`.
- [ ] Add commands: `OpenPauseMenuCommand`, `CloseSettingsCommand`, `SaveGameCommand`.
- [ ] Add `SaveRequested` event and `PauseSaveStatusText` property.
- [ ] Make `OpenSettingsCommand` remember whether the user came from `MainMenu` or `PauseMenu`.
- [ ] Make `CloseSettingsCommand` return to that remembered screen.
- [ ] Wire `MainWindow` to `SaveRequested` and save current game/window placement through existing `LocalSaveService`.
- [ ] Run the shell navigation tests and confirm they pass.
- [ ] Commit with `git commit -m "feat: add pause menu shell state"`.

## Task 3: Protect WPF Pause Menu Layout

**Files:**
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Update gameplay HUD assertions so `GameplayActionBar` has only one button and `GameplayMenuButton` binds to `OpenPauseMenuCommand`.
- [ ] Remove lookups/assertions for gameplay `NewGameButton` and `GameplaySettingsButton`.
- [ ] Add pause menu lookups/assertions for `PauseMenuView`, `PauseMenuPanel`, `PauseMenuActionList`, `PauseContinueButton`, `PauseSaveButton`, `PauseSettingsButton`, `PauseEndGameButton`, `PauseBackToMenuButton`, and `PauseSaveStatusText`.
- [ ] Assert pause commands bind to shell commands.
- [ ] Update settings screen assertions so `SettingsBackToMenuButton` uses `CloseSettingsCommand`.
- [ ] Run `dotnet test ColorLines.sln --filter "OccupiedCellsShowPieceBody|SettingsScreenUsesExistingGameSettingsCommands"` and confirm failures before XAML implementation.
- [ ] Commit with `git commit -m "test: protect pause menu wpf layout"`.

## Task 4: Implement WPF Pause Menu Layout

**Files:**
- Modify `src/ColorLines.Windows/MainWindow.xaml`

- [ ] Remove gameplay HUD direct `New Game` and `Settings` buttons.
- [ ] Bind `GameplayMenuButton` to `OpenPauseMenuCommand`.
- [ ] Add root-level `PauseMenuView` visible from `IsPauseMenuVisible`.
- [ ] Add named pause menu regions and buttons from the spec.
- [ ] Bind Settings back button to `CloseSettingsCommand`.
- [ ] Keep game-over buttons unchanged.
- [ ] Run WPF smoke tests for gameplay/settings and confirm they pass.
- [ ] Commit with `git commit -m "feat: add pause menu view"`.

## Task 5: Full Verification and Launch

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app for manual playtest.

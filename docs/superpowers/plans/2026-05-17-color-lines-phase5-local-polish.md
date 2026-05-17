# Color Lines Phase 5 Local Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add local desktop persistence for game state, high score, settings, and window placement.

**Architecture:** Keep serializable save contracts in the WPF layer because they include desktop-specific settings. Reuse `ColorLines.Core.Storage.GameSnapshot` for the game board and add a `LocalSaveService` abstraction so persistence can be tested with temporary files.

**Tech Stack:** .NET 8, C#, WPF, System.Text.Json, xUnit.

---

## Scope

This plan implements Phase 5 basics:

- Save and load high score, settings, game state, and window placement as JSON.
- Restore the most recent game state when the app starts.
- Update high score as score changes.
- Save on app close.
- Document run, test, publish, and local data behavior.

This plan does not add cloud sync, installers, or signed release packaging.

## Tasks

### Task 1: Add Local Save Contracts And Service

**Files:**
- Create: `src/ColorLines.Windows/Services/LocalSaveData.cs`
- Create: `src/ColorLines.Windows/Services/LocalSaveService.cs`
- Create: `tests/ColorLines.Tests/LocalSaveServiceTests.cs`

- [ ] Write failing tests for JSON save/load.
- [ ] Implement `LocalSaveData`, `WindowPlacementData`, and `LocalSaveService`.
- [ ] Verify tests pass.
- [ ] Commit with `feat: add local save service`.

### Task 2: Connect Save Data To GameViewModel

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Write failing tests for high-score initialization and save data export.
- [ ] Add `HighScore`, `CreateFromSave`, and `CreateSaveData`.
- [ ] Verify tests pass.
- [ ] Commit with `feat: connect view model save state`.

### Task 3: Save On Window Close And Restore On Startup

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `src/ColorLines.Windows/MainWindow.xaml.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Add high-score display to the UI.
- [ ] Load save data in `MainWindow`.
- [ ] Save game, settings, high score, and window size on close.
- [ ] Verify WPF smoke test passes.
- [ ] Commit with `feat: persist local desktop state`.

### Task 4: Documentation And Verification

**Files:**
- Modify: `README.md`

- [ ] Add local data and publish instructions to README.
- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch WPF app for smoke verification.
- [ ] Commit with `docs: add local desktop notes`.

## Self-Review

Spec coverage:

- High score: Task 2 and Task 3.
- Settings persistence: Task 1 through Task 3.
- Recent game restore: Task 1 through Task 3.
- Window size memory: Task 1 and Task 3.
- Maintainer README instructions: Task 4.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

Type consistency:

- `LocalSaveData`, `LocalSaveService`, `HighScore`, `CreateFromSave`, and `CreateSaveData` are used consistently.

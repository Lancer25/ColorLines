# Color Lines Game Over Panel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Show a complete end-of-run panel with final score, best score, summary text, and New Game action.

**Architecture:** Use existing `IsGameOver`, `Score`, `HighScore`, and `NewGameCommand`. Add formatted presentation properties in `GameViewModel`, then bind named WPF elements in the existing overlay.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM classes.

---

## Task 1: Game Over Presentation State

- [x] Add failing tests for `GameOverTitle`, `GameOverSummaryText`, `FinalScoreText`, and `BestScoreText`.
- [x] Run focused tests and verify they fail.
- [x] Implement the presentation properties in `GameViewModel` and raise score-related notifications.
- [x] Run focused tests and verify they pass.
- [x] Commit with `feat: expose game over summary text`.

## Task 2: Game Over Overlay UI

- [x] Add failing WPF smoke assertions for named overlay elements.
- [x] Run focused smoke test and verify it fails.
- [x] Update `MainWindow.xaml` game-over overlay with bound title, summary, final score, best score, and primary button.
- [x] Run WPF smoke tests and verify they pass.
- [x] Commit with `feat: upgrade game over panel`.

## Task 3: Verification

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app for manual verification.
- [x] Mark this plan complete and commit with `docs: mark game over panel complete`.

---

## Self-Review

- Spec coverage: presentation properties, WPF panel, tests, and verification are covered.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: game-over property and element names are used consistently.

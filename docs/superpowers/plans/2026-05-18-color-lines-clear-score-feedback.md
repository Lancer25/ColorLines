# Color Lines Clear Score Feedback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add stronger line-clear flash feedback and a visible score delta badge.

**Architecture:** Reuse `WasCleared` for board feedback and `TurnFeedback.ScoreDelta` for score feedback. Add small view model properties for badge visibility/text, then bind WPF overlays to those properties.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM classes.

---

## Task 1: Score Delta Badge State

- [x] Write failing tests for `ShowScoreDelta` and `ScoreDeltaBadgeText`.
- [x] Run the focused tests and verify they fail.
- [x] Add the two properties to `GameViewModel` and raise notifications when `Feedback` changes.
- [x] Run the focused tests and verify they pass.
- [x] Commit with `feat: expose score delta badge state`.

## Task 2: Clear Flash And Score Badge UI

- [x] Add failing WPF smoke assertions for `ClearPulseGlow` and `ScoreDeltaBadge`.
- [x] Run `OccupiedCellsShowPieceBody` and verify it fails.
- [x] Add `ClearPulseGlow` to the cell template and wrap score delta text in `ScoreDeltaBadge`.
- [x] Run WPF smoke tests and verify they pass.
- [x] Commit with `feat: add clear score feedback visuals`.

## Task 3: Full Verification

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app for manual verification.
- [x] Mark this plan complete and commit with `docs: mark clear score feedback complete`.

---

## Self-Review

- Spec coverage: score badge state, clear flash UI, smoke tests, and full verification are covered.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: `ShowScoreDelta`, `ScoreDeltaBadgeText`, `ClearPulseGlow`, and `ScoreDeltaBadge` are used consistently.

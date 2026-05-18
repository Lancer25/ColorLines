# Color Lines Animation Intensity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make animation intensity switchable and persistent.

**Architecture:** Add a toggle command and derived properties to `GameViewModel`. Suppress transient cell feedback flags in `Reduced` mode while preserving selection, reachable targets, and path preview. Add a WPF button in Settings.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM classes.

---

## Task 1: ViewModel Toggle And Persistence

- [x] Add failing tests for toggle command, label, save export, and save restore.
- [x] Run focused tests and verify they fail.
- [x] Add `ToggleAnimationCommand`, `IsFullAnimation`, and `AnimationToggleText`.
- [x] Run focused tests and verify they pass.
- [x] Commit with `feat: add animation intensity toggle`.

## Task 2: Reduced Mode Feedback Suppression

- [x] Add failing tests showing Reduced suppresses transient feedback but keeps planning feedback.
- [x] Run focused tests and verify they fail.
- [x] Gate transient flags in `RefreshFromState` behind `IsFullAnimation`.
- [x] Run focused tests and verify they pass.
- [x] Commit with `feat: reduce transient animation feedback`.

## Task 3: WPF Settings Control

- [x] Add failing WPF smoke assertion for `ToggleAnimationButton`.
- [x] Run focused smoke test and verify it fails.
- [x] Add the animation toggle button to Settings.
- [x] Run WPF smoke tests and verify they pass.
- [x] Commit with `feat: add animation settings control`.

## Task 4: Verification

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app for manual verification.
- [x] Mark this plan complete and commit with `docs: mark animation intensity complete`.

---

## Self-Review

- Spec coverage: toggle, persistence, reduced feedback behavior, WPF control, and verification are covered.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: property and command names are consistent.

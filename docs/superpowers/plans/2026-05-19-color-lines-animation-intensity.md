# Color Lines Animation Intensity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make animation intensity switchable and persistent.

**Architecture:** Add a toggle command and derived properties to `GameViewModel`. Suppress transient cell feedback flags in `Reduced` mode while preserving selection, reachable targets, and path preview. Add a WPF button in Settings.

**Tech Stack:** .NET 8, WPF, xUnit, existing MVVM classes.

---

## Task 1: ViewModel Toggle And Persistence

- [ ] Add failing tests for toggle command, label, save export, and save restore.
- [ ] Run focused tests and verify they fail.
- [ ] Add `ToggleAnimationCommand`, `IsFullAnimation`, and `AnimationToggleText`.
- [ ] Run focused tests and verify they pass.
- [ ] Commit with `feat: add animation intensity toggle`.

## Task 2: Reduced Mode Feedback Suppression

- [ ] Add failing tests showing Reduced suppresses transient feedback but keeps planning feedback.
- [ ] Run focused tests and verify they fail.
- [ ] Gate transient flags in `RefreshFromState` behind `IsFullAnimation`.
- [ ] Run focused tests and verify they pass.
- [ ] Commit with `feat: reduce transient animation feedback`.

## Task 3: WPF Settings Control

- [ ] Add failing WPF smoke assertion for `ToggleAnimationButton`.
- [ ] Run focused smoke test and verify it fails.
- [ ] Add the animation toggle button to Settings.
- [ ] Run WPF smoke tests and verify they pass.
- [ ] Commit with `feat: add animation settings control`.

## Task 4: Verification

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the WPF app for manual verification.
- [ ] Mark this plan complete and commit with `docs: mark animation intensity complete`.

---

## Self-Review

- Spec coverage: toggle, persistence, reduced feedback behavior, WPF control, and verification are covered.
- Placeholder scan: no incomplete placeholders remain.
- Type consistency: property and command names are consistent.

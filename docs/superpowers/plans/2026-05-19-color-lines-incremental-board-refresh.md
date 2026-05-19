# Color Lines Incremental Board Refresh Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep board cell view models stable and update them incrementally to reduce WPF visual tree churn.

**Architecture:** Convert `CellViewModel` from immutable-per-refresh to mutable-with-notifications while keeping row/column immutable. Update `GameViewModel.RefreshFromState()` to initialize cells once and update each cell in board order. Add a flood-fill helper for reachable targets to avoid per-cell path searches.

**Tech Stack:** .NET 8, WPF MVVM, xUnit.

---

## Task 1: Protect Stable Cell Instances

**Files:**
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Add tests proving selecting a cat keeps all `Cells` instances.
- [ ] Add tests proving moving a cat keeps all `Cells` instances.
- [ ] Add tests proving new game keeps all `Cells` instances.
- [ ] Run the new tests and confirm they fail before implementation.
- [ ] Commit with `git commit -m "test: protect stable board cell instances"`.

## Task 2: Make CellViewModel Mutable

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/CellViewModel.cs`

- [ ] Convert mutable cell state fields to backing fields and notifying properties.
- [ ] Keep `Row` and `Column` immutable.
- [ ] Add `Update(...)` that updates occupancy, selection, piece metadata, feedback flags, reachable target, and path preview.
- [ ] Keep `Empty(...)` and `Occupied(...)` factories by routing through the constructor.
- [ ] Run `dotnet test ColorLines.sln --filter "CellViewModelShowsEmptyCell|CellViewModelShowsOccupiedCatPiece"` and confirm pass.
- [ ] Commit with `git commit -m "feat: make cell view models updateable"`.

## Task 3: Refresh Board Incrementally

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/GameViewModel.cs`

- [ ] Add an `EnsureCellsInitialized()` helper that creates 81 cells only when the collection is empty.
- [ ] Replace `Cells.Clear()` and `Cells.Add(...)` in `RefreshFromState()` with calls to each existing cell's `Update(...)`.
- [ ] Keep `NextPieces` refresh unchanged.
- [ ] Run stable cell instance tests and confirm pass.
- [ ] Commit with `git commit -m "feat: refresh board cells incrementally"`.

## Task 4: Optimize Reachable Target Calculation

**Files:**
- Modify `src/ColorLines.Windows/ViewModels/GameViewModel.cs`

- [ ] Add a helper that computes all reachable empty positions with one breadth-first search from `selectedPosition`.
- [ ] Use that set in `RefreshFromState()` instead of calling `PathFinder.FindPath` per cell.
- [ ] Keep `PreviewPath` using `PathFinder.FindPath` because it needs the exact path.
- [ ] Run tests covering reachable targets and path preview.
- [ ] Commit with `git commit -m "perf: compute reachable targets once per refresh"`.

## Task 5: Full Verification and Launch

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the app for manual playtest.

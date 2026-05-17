# Color Lines Cat Piece Visuals Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the placeholder pieces into clearer cat-like WPF pieces and show next-piece previews visually.

**Architecture:** Keep piece presentation in the WPF ViewModel layer. Add a reusable `PieceViewModel` for board cells and next-piece previews, then bind XAML to that presentation model.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Scope

- Add `PieceViewModel` with name, label, face text, body brush, face brush, and ear brush.
- Make occupied board cells expose `PieceViewModel`.
- Make next-piece preview use the same visual model instead of plain strings.
- Update WPF board and next preview XAML to draw simple cat-like pieces with ears, round body, and readable face.
- Add WPF smoke coverage that occupied pieces render visible bodies.

## Tasks

### Task 1: Add Reusable Piece Presentation

**Files:**
- Create: `src/ColorLines.Windows/ViewModels/PieceViewModel.cs`
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Add failing tests for `PieceViewModel.FromPiece`.
- [ ] Implement `PieceViewModel`.
- [ ] Replace duplicated `CellViewModel` piece fields with `PieceViewModel? Piece`.
- [ ] Verify view model tests pass.
- [ ] Commit with `feat: add reusable piece presentation`.

### Task 2: Use Piece Presentation For Next Preview

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] Add failing tests proving `NextPieces` contains visual piece models.
- [ ] Change `NextPieces` to `ObservableCollection<PieceViewModel>`.
- [ ] Verify tests pass.
- [ ] Commit with `feat: show visual next piece previews`.

### Task 3: Draw Cat-Like Pieces In XAML

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [ ] Update board cell template to draw ears, round body, and face using `Piece` bindings.
- [ ] Update next preview template to draw mini cat pieces.
- [ ] Keep the selected ring and board layout stable.
- [ ] Verify WPF smoke test, all tests, and build pass.
- [ ] Commit with `feat: draw cat piece visuals`.

### Task 4: Launch Verification

- [ ] Run `dotnet test ColorLines.sln`.
- [ ] Run `dotnet build ColorLines.sln`.
- [ ] Launch the WPF exe and confirm it responds.
- [ ] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Round cat-head pieces: Tasks 1 and 3.
- Distinct color readability: Task 1.
- Next cat visual preview: Tasks 2 and 3.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

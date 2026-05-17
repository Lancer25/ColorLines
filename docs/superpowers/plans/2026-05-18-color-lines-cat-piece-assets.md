# Color Lines Cat Piece Assets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Render Color Lines pieces from project-local cat avatar PNG assets instead of WPF shape-only avatars.

**Architecture:** Keep core game logic unchanged. Add generated or deterministic PNG files under the CozyBoard theme, expose a per-piece asset URI from `PieceViewModel`, and update WPF templates to bind images for board and preview pieces.

**Tech Stack:** .NET 8, C#, WPF, xUnit, PNG assets.

---

## File Structure

- Create files in `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/`: one PNG per `PieceKind`.
- Modify `src/ColorLines.Windows/ViewModels/PieceViewModel.cs`: add `AssetPath`.
- Modify `src/ColorLines.Windows/MainWindow.xaml`: replace shape cat avatars with image-bound board and preview pieces.
- Modify `tests/ColorLines.Tests/GameViewModelTests.cs`: verify `AssetPath`.
- Modify `tests/ColorLines.Tests/WpfSmokeTests.cs`: verify `PieceImage` exists.

## Task 1: Add Asset Path Mapping

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/PieceViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [x] Add a failing test named `PieceViewModelProvidesThemeAssetPath`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter PieceViewModelProvidesThemeAssetPath`; expect compile failure for missing `AssetPath`.
- [x] Add `AssetPath` to `PieceViewModel` and map each `PieceKind` to `/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/<kind>.png`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter GameViewModelTests`; expect pass.
- [ ] Commit with `feat: map pieces to cat assets`.

## Task 2: Add First-Pass Cat PNG Assets

**Files:**
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/orange.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/gray.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/tuxedo.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/calico.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/black.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/white.png`
- Create: `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces/bluegray.png`
- Create: `tools/split_cat_sprite.py`

- [x] Generate or create transparent PNG cat avatar assets for every `PieceKind`.
- [x] Verify each PNG exists and is non-empty.
- [x] Commit with `art: add cat piece png assets`.

## Task 3: Render Image Assets In WPF

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `tests/ColorLines.Tests/WpfSmokeTests.cs`

- [x] Add a failing WPF smoke assertion for a named `PieceImage`.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect failure because `PieceImage` does not exist yet.
- [x] Replace board-piece WPF shape avatar elements with `<Image x:Name="PieceImage" Source="{Binding Piece.AssetPath}" ... />`.
- [x] Replace next-piece preview shape avatar elements with image-bound previews.
- [x] Run `dotnet test tests\ColorLines.Tests\ColorLines.Tests.csproj --filter WpfSmokeTests`; expect pass.
- [x] Commit with `feat: render cat piece images`.

## Task 4: Verify And Launch

- [x] Run `dotnet test ColorLines.sln`.
- [x] Run `dotnet build ColorLines.sln`.
- [x] Launch the WPF app.
- [x] Confirm the process responds.
- [x] Inspect `git status --short --branch`.

## Self-Review

Spec coverage:

- Project-local PNG assets: Task 2.
- Per-kind mapping: Task 1.
- WPF image rendering for board and preview: Task 3.
- No rule/save changes: all tasks stay in presentation and assets.

Placeholder scan:

- The plan contains no unresolved placeholder markers.

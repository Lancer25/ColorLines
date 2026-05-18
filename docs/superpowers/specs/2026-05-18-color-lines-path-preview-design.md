# Color Lines Path Preview Design

## Goal

Add a clear movement planning layer: after the player selects a cat, reachable empty cells are softly highlighted, and hovering a reachable target shows the exact path that cat will take.

## Player Experience

- Selecting an occupied cell keeps the existing selected-cat feedback.
- Empty cells that can be reached from the selected cat show a low-contrast warm highlight.
- Hovering a reachable empty cell shows a stronger path trail from the selected cat to that target.
- Hovering an unreachable empty cell does not draw a path and keeps the current invalid-click behavior if clicked.
- Moving a cat still uses the current turn resolution, scoring, spawn, clear, and save behavior.

This makes movement feel intentional without adding a full walking animation yet.

## Architecture

Use the existing core `PathFinder` as the source of truth. The WPF view model will derive preview-only board metadata from the current selected position and the current immutable `GameState`.

Add two presentation flags to `CellViewModel`:

- `IsReachableTarget`: true for empty cells with a valid path from the selected cat.
- `IsPathPreview`: true for cells in the currently hovered path.

Add hover commands to `GameViewModel`:

- `PreviewPathCommand`: receives a `CellViewModel`, computes the path if a cat is selected, and refreshes cells with preview markers.
- `ClearPreviewPathCommand`: clears preview markers when the pointer leaves a cell.

The WPF board cell template will bind these flags to subtle overlay brushes. The highlight must sit below the cat image layer so pieces remain readable.

## Testing

Unit tests should cover:

- Selecting a cat marks reachable empty cells.
- Blocked empty cells are not marked reachable.
- Hovering a reachable target marks the path cells.
- Clearing hover removes path preview markers without clearing the selected cat.

WPF smoke tests should cover:

- The board template contains named reachable and path preview overlays.

## Scope

This iteration does not animate the cat along the path. It only adds planning feedback. The design intentionally keeps path animation for a later feature so this step remains stable and easy to test.

## Review

No placeholders remain. The scope is limited to derived preview state and WPF visual bindings.

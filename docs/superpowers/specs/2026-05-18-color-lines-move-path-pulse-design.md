# Color Lines Move Path Pulse Design

## Goal

After a successful move, briefly highlight the route the cat used so the player can feel the move completed along the selected path.

## Player Experience

- The existing path preview still appears while hovering a reachable target.
- After clicking the target, the path cells receive a short blue-green pulse.
- The destination cell keeps the existing moved-to feedback.
- Invalid moves, selection changes, scoring, spawning, clearing, and saves stay unchanged.

This is a lightweight movement-feedback layer, not a full cat walking animation.

## Architecture

Use the existing `GameEventKind.PieceMoved` event, which already carries the path returned by `PathFinder`.

Add one presentation flag to `CellViewModel`:

- `WasMovePath`: true for cells in the most recent successful movement path.

`GameViewModel.StoreCellFeedback` will record all `PieceMoved` path positions in a new `movePathPositions` set. `RefreshFromState` will copy that flag into each cell. Current `WasMovedTo` remains the destination-only marker.

The WPF cell template will add a named `MovePathPulseGlow` overlay below cat images. It will animate opacity when `WasMovePath` is true.

## Testing

Unit tests should verify:

- A successful move marks the source, intermediate cells, and destination as move-path cells.
- Starting a new selection clears previous move-path feedback.

WPF smoke tests should verify:

- The board template contains `MovePathPulseGlow` and it is hidden by default.

## Scope

This iteration does not move the cat image along the path, delay turn resolution, or add sound. Those can be layered later once the feedback system is stable.

## Review

No placeholders remain. The feature is presentation-only and derives all path data from existing game events.

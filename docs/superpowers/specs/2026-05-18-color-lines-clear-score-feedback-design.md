# Color Lines Clear Score Feedback Design

## Goal

Make successful line clears feel rewarding by strengthening the cleared-cell flash and adding a short-lived score pop in the score panel.

## Player Experience

- When a line clears, cleared cells flash with a brighter warm-pink overlay.
- The score panel shows the existing `+N` score delta as a compact pop badge.
- The score pop is hidden when no points were scored.
- Movement, spawning, line detection, scoring, game-over, and saves remain unchanged.

## Architecture

Use the existing `TurnFeedback.HasScore`, `TurnFeedback.ScoreDelta`, and `CellViewModel.WasCleared` flags. This iteration adds presentation properties only:

- `GameViewModel.ShowScoreDelta`: true when `ScoreDeltaText` should be visible.
- `GameViewModel.ScoreDeltaBadgeText`: the same `+N` text, named for the UI badge.

The board already marks cleared cells through `WasCleared`; WPF will add a named `ClearPulseGlow` overlay and animate it when `WasCleared` is true. The score panel will wrap the delta text in a named badge container with opacity animation.

## Testing

Unit tests should verify:

- A scoring move exposes `ScoreDeltaBadgeText` and `ShowScoreDelta`.
- A non-scoring state hides the score delta badge.

WPF smoke tests should verify:

- `ClearPulseGlow` exists and is hidden by default.
- `ScoreDeltaBadge` exists and is hidden by default in a new game.

## Scope

This iteration does not add sound, combo counting, delayed clearing, or score-count-up animation. Those can follow after the presentation layer is stable.

## Review

No placeholders remain. The feature is a low-risk presentation enhancement over existing score and clear feedback state.

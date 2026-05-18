# Color Lines Animation Intensity Design

## Goal

Make the existing animation setting functional by letting players switch between `Full` and `Reduced` animation modes.

## Player Experience

- Settings shows the current animation mode.
- A button toggles between `Full` and `Reduced`.
- `Full` keeps the current transient movement, spawn, clear, reject, and score feedback.
- `Reduced` keeps essential interaction clarity such as selection, reachable targets, and hover path preview, but suppresses transient celebratory effects.
- The selected mode is saved and restored.

## Architecture

Use the existing `AnimationIntensity` save field. Add:

- `ToggleAnimationCommand`
- `IsFullAnimation`
- `AnimationToggleText`

`GameViewModel.RefreshFromState` will only expose transient feedback flags when `IsFullAnimation` is true. Path preview and selected/reachable states remain available in both modes.

## Testing

Unit tests should verify:

- The command toggles `AnimationIntensity` between `Full` and `Reduced`.
- Save/restore preserves `Reduced`.
- Reduced mode suppresses transient move/spawn/clear/reject flags.
- Reduced mode still exposes reachable target and path preview flags.

WPF smoke tests should verify:

- The animation toggle button exists and is bound.

## Scope

This iteration does not add new animation types or a full preferences screen. It only makes the existing animation setting functional.

## Review

No placeholders remain. The feature is small, persistent, and presentation-focused.

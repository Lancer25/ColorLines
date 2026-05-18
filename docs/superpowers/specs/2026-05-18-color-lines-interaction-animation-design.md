# Color Lines Interaction Animation Design

## Goal

Add lightweight WPF interaction animations that make selecting, moving, spawning, clearing, and invalid target clicks feel responsive.

## Animation Direction

- Selected cat: gently lifts upward with the existing warm halo and shadow.
- Move target: blue glow stays soft while the cat gives a short settle/pop.
- Spawned cat: green glow plus a small pop-in scale.
- Cleared cell: pink glow fades through the existing feedback state.
- Rejected target: red feedback remains subtle and should not shake the entire board.

Animations should be short, local, and non-blocking. They must not change game rules, state transitions, saves, or scoring.

## Architecture

Keep interaction state in `CellViewModel` as it is today. Add WPF `RenderTransform` layers to the board-piece visual tree and attach `DataTrigger.EnterActions` storyboards to existing flags: `IsSelected`, `WasMovedTo`, `WasSpawned`, and `WasRejectedTarget`.

The image layer should be wrapped in a named `PieceActor` so future path movement can animate one container without changing PNG rendering.

## Testing

WPF smoke tests should verify:

- The piece image still exists and is visible.
- A named `PieceActor` exists.
- The actor has a `TransformGroup` containing scale and translate transforms.
- Existing resource packaging tests remain green.

## Scope

This iteration does not implement path-by-path movement. It only adds local feedback animations and the visual structure needed for future path animation.

## Review

No unresolved placeholders remain. This is a presentation-only change.

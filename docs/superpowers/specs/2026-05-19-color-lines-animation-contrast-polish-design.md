# Color Lines Animation Contrast Polish Design

## Context

Reduced animation is wired into the board feedback, but the player-facing difference is too subtle. The current reduced mode still displays low-opacity transient feedback for movement, clearing, spawning, rejected targets, and score changes, so it feels close to full mode during normal play.

## Design

Make the two animation modes intentionally distinct:

- Full animation keeps and slightly strengthens transient feedback so the game feels playful.
- Reduced animation removes non-essential transient feedback and keeps only core interaction affordances: selected cat, reachable targets, and path preview.

## Affected Feedback

In Reduced mode, these transient layers should stay hidden instead of showing low-opacity static cues:

- `MovePathPulseGlow`
- `ClearFeedbackGlow`
- `ClearPulseGlow`
- `RejectFeedbackGlow`
- `SpawnFeedbackGlow`
- `MoveFeedbackGlow`
- `ScoreDeltaBadge`

Full mode can use stronger starting opacity values for those same fade animations. `ReachableTargetGlow`, `PathPreviewGlow`, selected cat highlight, piece base, and piece shadow remain unchanged in both modes.

## Acceptance Criteria

- Full mode feels more lively than the previous version.
- Reduced mode is visibly calmer and avoids transient flash effects.
- Reduced mode still leaves core move planning readable.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

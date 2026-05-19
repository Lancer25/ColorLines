# Color Lines Animation Feedback Polish Design

## Context

The game now has polished main menu, gameplay HUD, settings lobby, and game-over flow. The next UX gap is that the settings screen exposes `Animation: Full/Reduced`, but the board feedback visuals are still mostly authored as full animated feedback.

This step makes the animation setting meaningful without changing the game rules.

## Recommendation

Honor animation intensity in board feedback.

Full animation should keep the current playful pulses and fades. Reduced animation should avoid motion/fade storyboards for transient feedback and instead use short-lived, static visual markers while the view model flags are active. Always-visible affordances such as reachable targets and path preview can remain static because they are not motion-heavy and are important for readability.

## Feedback Layers

The following transient feedback layers should respect `Game.IsFullAnimation`:

- `MovePathPulseGlow`
- `ClearFeedbackGlow`
- `ClearPulseGlow`
- `RejectFeedbackGlow`
- `SpawnFeedbackGlow`
- `MoveFeedbackGlow`
- `PieceScaleActor` movement/spawn opacity animations
- `ScoreDeltaBadge`

For Full animation, keep storyboard-based feedback. For Reduced animation, avoid storyboard enter actions and show a low-opacity static cue for the same transient flags.

## Interaction Requirements

- Selecting a cat still shows reachable targets.
- Hovering a reachable empty cell still previews the path.
- Moving, spawning, clearing, rejecting an invalid target, and scoring still provide feedback.
- Toggling animation in settings changes the feedback mode without restarting the app.
- Sound, score, line clearing, and save data behavior do not change.

## Test Requirements

Add smoke-test coverage that protects the reduced-animation wiring in XAML:

- The named transient feedback layers have triggers that reference `Game.IsFullAnimation`.
- `ScoreDeltaBadge` uses `Game.IsFullAnimation` when deciding animated versus reduced feedback.
- The existing persistent-opacity regression tests continue to pass.
- Existing view-model tests for animation setting and gameplay behavior continue to pass.

## Non-goals

This step does not add new animation settings, sliders, accessibility pages, new cat assets, or rule changes. It also does not redesign the board layout.

## Acceptance Criteria

- Full mode keeps the lively board feedback.
- Reduced mode avoids motion-heavy board feedback while remaining readable.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

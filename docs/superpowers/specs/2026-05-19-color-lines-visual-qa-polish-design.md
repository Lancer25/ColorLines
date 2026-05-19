# Color Lines Visual QA Polish Design

## Context

The main menu, gameplay HUD, and settings lobby now share a stronger cozy game identity. The remaining visible rough edge is the game-over modal: it still uses older raw button styling and offers only a new-game action. This makes the end-of-run experience feel less connected to the newer navigation flow.

This step is a focused visual QA polish pass, not a feature expansion.

## Recommendation

Polish the game-over modal and navigation closure.

The game-over overlay should use the same game-style button language as the main menu, gameplay HUD, and settings lobby. It should keep the final score summary, offer a primary New Game action, and add a secondary Back to Menu action so players have a clear end-of-run choice.

## Scope

Update only the WPF window layout and WPF smoke tests:

- Name the game-over dialog and action bar so tests can protect the structure.
- Restyle the modal using existing brushes and button styles.
- Add `GameOverNewGameButton` bound to `NewGameCommand`.
- Add `GameOverMenuButton` bound to `BackToMenuCommand`.
- Keep the overlay outside the content margins so it covers the full window.

## Non-goals

This step does not change game rules, scoring, board generation, save data, cat assets, audio, or shell state semantics. It also does not add a new modal system.

## Test Requirements

Update smoke tests to protect:

- `GameOverDialog` exists inside `GameOverOverlay`.
- `GameOverActionBar` exists and contains at least two actions.
- `GameOverNewGameButton` binds to `ShellViewModel.NewGameCommand`.
- `GameOverMenuButton` binds to `ShellViewModel.BackToMenuCommand`.
- Game-over buttons use existing menu/gameplay styles through their `Tag` values.
- The overlay remains a root-level sibling outside `GameplayView` margin.

## Acceptance Criteria

- Game Over visually matches the newer game UI.
- Players can start a new game or return to the main menu from Game Over.
- Existing overlay coverage behavior remains protected.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

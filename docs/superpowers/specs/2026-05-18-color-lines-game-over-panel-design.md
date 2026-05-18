# Color Lines Game Over Panel Design

## Goal

Upgrade the game-over overlay into a complete end-of-run summary without changing game rules or save behavior.

## Player Experience

- When the board is full, the overlay shows a clear game-over title.
- The overlay shows the final score and best score.
- The overlay includes a short message encouraging a new run.
- The existing New Game button remains the primary action.
- The overlay keeps the board dimmed behind it.

## Architecture

Use the existing `GameViewModel.IsGameOver`, `Score`, `HighScore`, and `NewGameCommand`. Add presentation-only strings to make the overlay testable and easier to maintain:

- `GameOverTitle`: fixed title text.
- `GameOverSummaryText`: short body text.
- `FinalScoreText`: formatted from `Score`.
- `BestScoreText`: formatted from `HighScore`.

The WPF overlay will bind to these properties and expose named elements for smoke tests.

## Testing

Unit tests should verify:

- A game-over view model exposes the expected title, summary, final score, and best score text.
- New game still clears game-over state.

WPF smoke tests should verify:

- The game-over panel contains named final-score and best-score text elements.

## Scope

This iteration does not add leaderboards, achievements, animations, or confirmation prompts. It only improves the game-over presentation.

## Review

No placeholders remain. The design is presentation-only and uses existing game state.

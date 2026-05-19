# Color Lines Menu Safety Design

## Goal

Make the menu flow match a traditional game shell: the main menu should not expose a separate new-game entry, and returning to the main menu from active gameplay must warn that unsaved progress can be lost.

## Scope

- Remove the main menu `New Game` button.
- Keep `Continue`, `Settings`, and `Exit` on the main menu.
- Change the pause menu `Return to Main Menu` action into a two-step flow.
- Do not change save file format, scoring, board rules, or game-over actions.

## Interaction Design

The pause menu keeps its existing actions: continue, save, settings, end game, and return to main menu. When the player chooses `Return to Main Menu`, the pause menu shows an inline confirmation panel instead of immediately leaving gameplay.

The confirmation panel states that returning to the main menu will lose current unsaved progress. It offers two choices:

- `Stay in Game`: hides the confirmation and keeps the pause menu open.
- `Return Anyway`: returns to the main menu.

Continuing the game, starting a new game, opening settings, or returning after confirmation clears any pending confirmation state.

## ViewModel Design

`ShellViewModel` owns a boolean confirmation state because this is shell navigation, not board gameplay. It exposes:

- `IsReturnToMenuConfirmVisible`
- `RequestBackToMenuCommand`
- `ConfirmBackToMenuCommand`
- `CancelBackToMenuCommand`

`BackToMenuCommand` remains available for safe contexts such as game-over, where the current run is already finished.

## Testing

Add focused tests for shell behavior and WPF smoke coverage:

- Main menu no longer contains `MenuNewGameButton`.
- Requesting return from pause menu does not leave pause immediately.
- Confirming return moves to the main menu.
- Cancelling keeps the player in the pause menu.
- The pause menu return button binds to the request command and exposes the confirmation panel.

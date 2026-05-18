# Color Lines Main Menu Shell Design

## Goal

Move the Windows app from a single crowded game screen to a traditional game flow with a dedicated main menu, a focused gameplay screen, and a separate settings surface.

## Problem

The current right rail tries to hold score, next cats, new game, status text, sound, animation, and theme information while the board is also on screen. As the game grows, that rail becomes crowded and visually unstable. Settings should not compete with the board during play.

## Approved Direction

Use a shell-style WPF layout with three modes:

- `MainMenu`: first screen after launch.
- `Playing`: actual Color Lines board and compact gameplay HUD.
- `Settings`: settings screen reachable from the main menu and from gameplay.

The app remains one desktop window. This avoids extra window management and keeps local save behavior simple.

## Main Menu

The main menu should feel like a traditional desktop game entry screen, not a marketing page. It should show:

- Game title: `Color Lines`.
- Highest score.
- Primary actions:
  - `Continue`
  - `New Game`
  - `Settings`
  - `Exit`
- A small preview of the cozy cat/board identity using existing WPF styling and current cat assets.

`Continue` loads the current saved or in-memory game and enters `Playing`. If there is no explicit saved game distinction yet, it may simply enter the current restored game. `New Game` resets the game state and enters `Playing`.

## Gameplay Screen

The gameplay screen should focus on play:

- Keep the 9x9 board as the primary element.
- Keep score, best score, next cats, and status text.
- Remove the large settings panel from the always-visible right rail.
- Add compact navigation actions:
  - `Menu`
  - `New Game`
  - `Settings` as a compact action that navigates away from the board instead of expanding inline.

The board and cat piece presentation from the recent visual upgrade should remain.

## Settings Screen

Settings should live outside the active board view. The first version includes only existing settings:

- Sound enabled toggle.
- Animation intensity toggle.
- Theme name display for `Cozy Board`.
- Back action.

Changing settings should keep using the existing `GameViewModel` settings properties and commands so save/export behavior remains unchanged.

## View Model Shape

Add a lightweight shell view model in the WPF layer. It owns navigation state and wraps the existing `GameViewModel`.

Suggested responsibilities:

- Current screen/mode.
- Commands for `Continue`, `New Game`, `OpenSettings`, `BackToMenu`, `BackToGame`, and `Exit`.
- Expose the active `GameViewModel` for existing board and HUD bindings.

The core game model remains untouched.

## Save Behavior

Local save JSON should not change in this iteration. The shell can save through the existing `GameViewModel.CreateSaveData(...)` path when the window closes. Window size persistence stays as-is.

## Testing

Add focused WPF and view-model tests:

- App starts on the main menu.
- Continue switches to the gameplay screen.
- New Game switches to gameplay and resets score/status through the existing game command.
- Settings switches to the settings screen.
- Settings controls still bind to the existing sound and animation commands.
- Game over overlay still covers the full window only when gameplay is visible.

Manual verification should launch the app and check:

- Default launch shows menu.
- Continue enters playable board.
- Settings can be opened and exited without layout crowding.
- Closing the app still persists high score, settings, game state, and window size.

## Non-Goals

This iteration does not add:

- New gameplay rules.
- Difficulty modes.
- New cat assets.
- Separate settings windows.
- Web support.
- A top application menu bar.

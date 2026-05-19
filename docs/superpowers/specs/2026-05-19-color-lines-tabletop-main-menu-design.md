# Color Lines Tabletop Main Menu Design

## Goal

Redesign the main menu into a modern tabletop-style game entrance that feels polished, cozy, and clearly connected to the cat-and-board identity of Color Lines.

## Problem

The current main menu is functional but too plain. It reads like an application form rather than a game title screen. The preview board is small, the buttons feel generic, and the layout does not create a strong first impression.

## Approved Direction

Use a tabletop composition:

- A warm game-table background fills the first screen.
- A large decorative board preview acts as the visual anchor.
- The title sits near the board instead of floating as plain text.
- Main actions live in a clean vertical command stack.
- Secondary information is compact and quiet.

This should feel like entering a cozy desktop board game, not like opening a settings-heavy utility.

## Layout

The screen remains a single WPF window. The main menu uses a two-zone composition:

### Hero Board Zone

The left side or center-left holds the main visual object:

- A larger board preview, roughly 4x4 or 5x5 visual cells.
- Several cat PNGs placed inside preview cells.
- Soft board shadow and warm board frame, reusing current `CozyBoard` theme resources.
- Title `Color Lines` visually grouped with the board.
- A short subtitle is allowed, but it must stay secondary.

The preview is decorative only. It should not use gameplay state or accept input.

### Command Zone

The right side holds the menu commands:

- `Continue` as the primary action.
- `New Game`.
- `Settings`.
- `Exit`.

Buttons should be taller and more game-like than standard form buttons. The primary action should use the accent color. Secondary actions should be calmer but clearly clickable.

### Status Strip

Small status text should sit near the command stack:

- Best score.
- Current theme.
- Sound state.
- Animation mode.

These should not be separate large cards. They are supporting details.

## Visual Style

Use WPF-native styling only:

- No new cat images.
- No external asset downloads.
- No custom SVG hero illustration.
- No gradient-orb decoration.

The menu may use layered `Border`, `LinearGradientBrush`, `DropShadowEffect`, and existing PNG cat assets.

Color should stay warm but not become a flat beige/yellow screen. Use contrast from wood brown, cream, muted teal, and cat colors.

## Interaction

The menu buttons use existing shell commands:

- `ContinueButton` -> `ContinueCommand`.
- `MenuNewGameButton` -> `NewGameCommand`.
- `MenuSettingsButton` -> `OpenSettingsCommand`.
- `ExitButton` -> `ExitCommand`.

No gameplay logic changes.

## Testing

Update WPF smoke tests to protect the new structure:

- `MainMenuView` is visible on launch.
- `GameplayView` is collapsed on launch.
- The hero board preview exists and contains cat preview images.
- The command zone exists and all four commands are bound.
- Status strip text can bind to the existing `Game` settings.

Manual verification should check:

- Default launch looks like a game title screen.
- Buttons fit at minimum window size.
- Continue and New Game still enter gameplay.
- Settings still opens the separate settings screen.

## Non-Goals

This iteration does not:

- Change gameplay rules.
- Add animated menu background.
- Add new assets.
- Add a separate profile system.
- Add save-slot selection.
- Change local save JSON.

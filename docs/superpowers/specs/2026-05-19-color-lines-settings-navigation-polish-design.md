# Color Lines Settings and Navigation Polish Design

## Context

The main menu and gameplay screen now have a stronger game identity. The settings screen is the remaining weak surface: it is a centered form card with plain toggle buttons and a generic Back button. It works, but it does not feel like part of the same cozy tabletop game shell.

This step polishes settings and navigation without changing game rules, save data, or the existing shell state model.

## Recommendation

Use a dedicated settings lobby layout.

The settings screen should feel like a small game menu screen rather than a desktop dialog. It should share the visual language of the main menu and gameplay HUD: warm tabletop colors, compact panels, stable spacing, clear action hierarchy, and buttons that match the current menu styles.

## Alternatives Considered

### A. Dedicated settings lobby

Create a wider settings screen with a title area, grouped setting rows, and a clear action strip. This is recommended because it brings settings into the game UI system while staying simple to maintain.

### B. Modal settings overlay above the current screen

This would preserve context, but it adds overlay state and interaction complexity that the current shell does not need yet.

### C. Minimal restyle of the current centered card

This is the smallest change, but it would leave the navigation and settings experience feeling less polished than the main menu and gameplay screen.

## Layout Design

Keep `SettingsView` as a separate shell screen. Replace the single narrow card with a named settings shell:

- `SettingsShell`: the full settings surface.
- `SettingsHeader`: title, short status copy, and a small current-theme summary.
- `SettingsContentPanel`: the main settings panel.
- `SettingsOptionList`: grouped setting rows.
- `AnimationSettingRow`: animation mode row with current value and toggle command.
- `SoundSettingRow`: sound row with current value and toggle command.
- `SettingsActionBar`: Back to Menu and New Game actions.

The screen should not duplicate the gameplay board. It should be calm and easy to scan, with enough visual weight to feel intentional.

## Navigation Requirements

The existing commands remain the source of truth:

- Main menu Settings opens `SettingsView`.
- Gameplay Settings opens `SettingsView`.
- Back returns to the main menu through `BackToMenuCommand`.
- New Game starts a new game through `NewGameCommand`.
- Animation toggle uses `Game.ToggleAnimationCommand`.
- Sound toggle uses `Game.ToggleSoundCommand`.

No new navigation state is required in this step.

## Visual Requirements

- Use existing menu/gameplay brushes where possible.
- Add only small theme resources if a settings-specific background or row surface is useful.
- Use the existing `MenuPrimaryButton`, `MenuSecondaryButton`, or `GameplayCompactButton` styles instead of raw button colors.
- Use stable row heights so changing values such as `Animation: Full` and `Sound enabled: True` do not shift the layout.
- Keep text readable at the current desktop window size.
- Avoid nested cards, decorative blobs, and oversized form controls.

## Test Requirements

Update WPF smoke tests to protect the settings structure:

- `SettingsView` becomes visible after `OpenSettingsCommand`.
- `SettingsShell`, `SettingsHeader`, `SettingsContentPanel`, `SettingsOptionList`, `AnimationSettingRow`, `SoundSettingRow`, and `SettingsActionBar` exist.
- Settings buttons bind to the existing shell/game commands.
- The old gameplay settings panel remains absent.
- Main menu and gameplay navigation tests continue to pass.

## Non-goals

This step does not add new settings, keyboard shortcut configuration, language selection, theme selection, volume sliders, or web-specific settings. It does not change save data or command behavior.

## Acceptance Criteria

- Settings feels visually connected to the main menu and gameplay HUD.
- Settings controls are easier to scan and no longer look like plain desktop defaults.
- Navigation remains simple and predictable.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

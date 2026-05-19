# Color Lines Main Menu Polish Design

## Goal

Polish the tabletop main menu into a more finished modern casual-game title screen before moving on to gameplay HUD and settings-page refinements.

This is phase A of the approved sequence:

1. Main menu polish and lightweight interaction feel.
2. Gameplay screen polish.
3. Settings and navigation polish.

## Problem

The current tabletop menu is better than the first plain menu, but it still feels like a structured WPF layout rather than a finished game title screen. The main issues are:

- The background does not yet feel like a designed tabletop scene.
- The board preview is large but still too literal and flat.
- Buttons are sized better, but still look like standard WPF controls.
- Title, board, and commands do not yet feel composed as one scene.
- There is little interaction feedback beyond basic clicks.

## Design Direction

Create a richer title-screen composition while staying WPF-native and asset-light:

- Use a layered tabletop background behind the whole menu.
- Make the board preview feel like the hero object with depth, shadow, and better framing.
- Give the title a stronger relationship to the hero board.
- Use a custom game-menu button style for primary and secondary commands.
- Add subtle hover/pressed/focus feedback.
- Add only trigger-based button feedback in this phase; no load-loop or ambient motion.

The result should feel cozy, modern, and playful without becoming noisy.

## Main Menu Structure

The existing `MainMenuView` remains the root menu surface. It should contain named regions:

- `MenuBackdrop`: full menu background layer.
- `MenuHeroArea`: title and board composition.
- `MenuHeroBoard`: large decorative board preview.
- `MenuCommandPanel`: command stack.
- `MenuStatusStrip`: compact supporting information.

These names support WPF smoke tests and future maintenance.

## Visual Requirements

### Backdrop

The background should be more deliberate than a flat app color:

- A warm tabletop surface using a subtle linear gradient.
- A darker vignette or edge band is acceptable if implemented with WPF gradients.
- No orb/blob decoration.
- No external image dependency.

### Hero Board

The hero board should remain a 5x5 decorative preview but look more dimensional:

- Keep current cat PNGs.
- Add a soft shadow or board base.
- Add an inner board surface distinct from the outer frame.
- Cells should look inset and consistent.
- Cat images should sit visually inside cells, not float randomly.

The board remains decorative and non-interactive.

### Title

The title should feel part of the scene:

- Keep `Color Lines` as the main title.
- Increase polish through spacing, sizing, and a subtitle treatment.
- Avoid oversized text that collides at minimum window size.

### Command Buttons

Create reusable WPF styles for title-screen command buttons:

- Primary button style for `Continue`.
- Secondary button style for `New Game`, `Settings`, and `Exit`.
- Stable height, padding, font weight, hover state, pressed state, and focus state.
- Add explicit menu button brushes to `CozyBoard.xaml` so the style is reusable.

Buttons must not resize or shift layout on hover.

### Status Strip

The status strip should read as a small HUD:

- Best score.
- Theme.
- Animation mode.
- Sound state.

It should be compact, aligned, and visually secondary.

## Interaction Feel

Add lightweight WPF interaction feedback:

- Button hover brightens or deepens the surface.
- Button press gives a subtle visual compression or color change.
- Main menu does not add load-in animation in this phase.

No continuous animation is required in this phase.

## Testing

Update smoke tests to protect:

- `MenuBackdrop` exists and spans the main menu.
- `MenuHeroArea` exists.
- `MenuHeroBoard` remains large.
- Command buttons use the expected minimum heights.
- Menu preview cats are still present.
- Command bindings are unchanged.

Manual verification:

- Launch shows a visually stronger title screen.
- Menu fits at minimum window size.
- Buttons have hover/pressed feedback.
- Continue, New Game, Settings, and Exit still route through existing shell commands.

## Non-Goals

This phase does not:

- Change gameplay rules.
- Change the gameplay screen.
- Change the settings screen.
- Add new image assets.
- Add audio changes.
- Add save slots.
- Change local save JSON.

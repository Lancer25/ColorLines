# Color Lines Main Screen Visual Upgrade Design

## Goal

Upgrade the Windows main screen from a functional prototype into a more polished cozy board-game interface without changing gameplay rules, save format, or cat artwork.

## Approved Direction

Use the existing `CozyBoard` theme and WPF layout, but make the screen feel more intentional:

- The board should read as a physical game board with more depth and less flat yellow surface area.
- Cells should stay highly readable while feeling softer and less grid-heavy.
- Cat pieces should look seated on the board with consistent shadow, base, and selected-state treatment.
- The right rail should become quieter and easier to scan, with score, next cats, primary action, and settings clearly separated.
- Status text should remain stable in size but feel like auxiliary guidance instead of the visual focus.

## Scope

This iteration changes presentation only:

- WPF resource colors and brushes.
- Board, cell, piece-shadow, next-preview, panel, and button styling.
- WPF smoke tests that protect the presence of named visual elements and controls.

This iteration does not:

- Change core rules, scoring, line detection, pathfinding, or random spawning.
- Regenerate or replace cat PNG assets.
- Change the local save JSON contract.
- Add new windows, menus, difficulty settings, or gameplay modes.

## Visual Requirements

### Board

The board container should have a warmer, deeper surface using layered borders or gradients. It should still fit the existing 9x9 layout and remain responsive through the current `Viewbox`.

### Cells

Cells should have a calmer background, smaller visual noise, and a predictable hover state. Empty cells should remain obvious, but the page should no longer feel dominated by 81 bright outlined squares.

### Pieces

Cat images remain the source of piece identity. Presentation should improve through WPF-only treatment: softer shadow, subtle pedestal/base, and clearer selected glow. No custom drawing should cover or distort the cats.

### Right Rail

The right rail should feel like a compact game HUD:

- Title and status at the top.
- Score panel with high score and score delta.
- Next Cats preview with three compact preview tiles.
- New Game as the strongest action.
- Settings as lower-priority controls.

### Accessibility And Stability

Text must fit within its containers at the current minimum window size. The status region remains fixed-height so changing guidance text does not shift the layout. Reduced-animation behavior remains unchanged.

## Testing

Add focused smoke coverage for the visual structure rather than screenshot-perfect assertions:

- The main board container has a stable name so future tests can locate it.
- The right rail has a stable name.
- The New Game button has a stable name.
- The existing animation and sound controls remain discoverable.

Manual verification should include launching the app and checking the default window and minimum-size layout.

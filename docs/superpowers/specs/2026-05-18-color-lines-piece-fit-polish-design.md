# Color Lines Piece Fit Polish Design

## Goal

Make PNG cat pieces feel seated inside the board cells instead of pasted on top of the grid.

## Visual Direction

- Keep the CozyBoard palette and the current PNG cat assets.
- Slightly reduce board-piece size so ears and face have comfortable padding inside each cell.
- Add a subtle named ground shadow under each occupied piece.
- Keep move, spawn, clear, and rejected feedback as soft cell glows behind the piece.
- Make selected pieces read as lifted with a warm halo, not a thick outline.
- Make next-piece previews use the same visual proportion as board pieces.

## Architecture

This remains a XAML-only presentation polish. `GameViewModel`, core rules, saves, and PNG assets do not change. WPF smoke tests should assert the board template contains the named image and shadow layers.

## Testing

- Update WPF smoke tests to verify `PieceImage` and `PieceShadow` exist and use polished dimensions.
- Run full test and build after the XAML change.

## Review

No unresolved placeholders remain. The work is intentionally scoped to visual fit and template polish.

# Color Lines Soft Visual Refresh Design

## Goal

Improve the current WPF prototype visual quality by replacing hard debug-like rings with softer game feedback, and by making the cat pieces read as cute cat avatars instead of labeled colored balls.

## Visual Direction

The board should keep the cozy wooden theme, but cell feedback should become a soft layer under or around the piece:

- Move target: subtle blue glow under the piece.
- Spawned piece: subtle green glow under the piece.
- Cleared cell: warm pink fill/glow on the cell.
- Rejected target: brief red-tinted cell fill.
- Selected piece: lifted cat avatar with a soft gold halo and shadow.

Hard thick outlines should be removed from feedback states. Feedback should not compete with the piece silhouette.

## Cat Avatar Direction

Cat pieces stay WPF-native for now so the project remains easy to maintain. Each piece should use:

- A rounded head/body shape.
- Two ears with inner ear accents.
- Small face details drawn with simple shapes, not text symbols.
- A subtle highlight and shadow.
- Color variants from `PieceKind`, while keeping facial contrast readable.

The next-piece preview should reuse the same visual language at smaller scale.

## Scope

This refresh is limited to visual presentation and smoke-test coverage. Core rules, saves, scoring, and interaction state remain unchanged.

## Testing

Existing view model tests continue to cover feedback state. WPF smoke tests should verify that the visual tree contains the upgraded piece and soft feedback layers.

## Review

No placeholder requirements remain. The scope is intentionally limited to a WPF-native visual refresh, leaving bitmap cat assets and timeline animations for a later iteration.

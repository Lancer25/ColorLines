# Color Lines Cat Piece Assets Design

## Goal

Replace WPF shape-only cat pieces with project-local PNG cat avatar assets so the game feels more polished, cute, and distinctive.

## Visual Direction

The pieces should read as a consistent set of cute round cat avatars:

- Front-facing cat heads with soft shading, ears, eyes, nose, cheeks, and a small smile.
- Transparent backgrounds so the board feedback glow remains visible.
- One visual variant per `PieceKind`: Orange, Gray, Tuxedo, Calico, Black, White, and BlueGray.
- Same style for board pieces and next-piece previews.

The existing soft board feedback remains. The image asset should be the main character layer, while glow and selection effects stay in WPF for interactivity.

## Architecture

Assets live under `src/ColorLines.Windows/Assets/Themes/CozyBoard/pieces`. `PieceViewModel` exposes a pack URI for each `PieceKind`, and `MainWindow.xaml` binds `Image.Source` to that URI. The older WPF shape avatar remains unnecessary once images are wired in.

This keeps the future web port straightforward: the same logical `PieceKind -> asset path` mapping can be reused by a web renderer.

## Asset Pipeline

Use the built-in image generation flow to produce a first-pass cat sprite sheet or individual PNGs, then store final project-bound PNGs in the workspace. If a generated image has a flat chroma-key background, remove it locally and save transparent PNGs.

If generation cannot reliably produce clean transparent individual assets, create deterministic placeholder PNGs for the first integration pass, but keep the WPF code asset-driven so better art can be swapped without code changes.

## Testing

- Unit tests verify `PieceViewModel` exposes `AssetPath`.
- WPF smoke tests verify board cells render a named `PieceImage`.
- Existing game logic and save tests remain unchanged.

## Scope

This change does not alter rules, scoring, saves, or board feedback state. It only changes piece presentation from WPF shape avatars to image-backed avatars.

## Review

No unresolved placeholders remain. The first implementation target is asset-driven rendering with project-local PNG files; higher-fidelity art iteration can continue after the pipeline is in place.

# Color Lines Gameplay HUD Polish Design

## Context

The game now opens through a dedicated main menu, and the playing screen is the next visual weak point. The board and cat pieces are functional, but the current playing layout still feels like a desktop utility: a large board on the left and a stacked information rail on the right with a repeated title, changing status text, score card, next queue, and settings controls.

The goal for this step is to make the in-game screen feel more like a modern casual puzzle game while preserving the stable board, rules, save flow, cat assets, path preview, clear feedback, game-over overlay, and settings behavior.

## Recommendation

Use a board-first game HUD layout.

Compared with a full redesign or a menu-style screen, this is the best next step because it improves the moment-to-moment play experience without destabilizing the game logic. The board should remain the dominant visual surface. Score, best score, next cats, status, and actions should become a compact game HUD around the board instead of a tall app sidebar.

## Alternatives Considered

### A. Board-first compact HUD

Keep the board as the largest object and restyle the surrounding controls into a denser, game-like HUD. This is the recommended option because it fixes the current cramped right rail without changing gameplay flow.

### B. Full-screen board with floating overlays

Make the board nearly full-window and float score/actions over the board area. This could feel more immersive, but it has higher overlap risk and makes the current game-over and path feedback layers harder to reason about.

### C. Keep the right rail and only improve styling

This is the smallest change, but it would not solve the core problem: the game screen still reads as a form layout rather than a playful game scene.

## Layout Design

The playing screen remains a single WPF view named `GameplayView`, but its internal structure changes from `MainBoardFrame + RightRail` into a game shell with clear named regions:

- `GameplayShell`: the full playing surface.
- `GameplayBoardArea`: the board side, centered and visually dominant.
- `GameplayHudPanel`: a compact HUD side panel for score, next cats, and actions.
- `GameplayStatusBanner`: a fixed-height status banner so selection prompts no longer resize the layout.
- `GameplayScoreBlock`: a tighter score block showing score, best score, and score delta.
- `GameplayNextCatsBlock`: a compact next-cats queue with the same cat images.
- `GameplayActionBar`: navigation and session actions such as Menu, New Game, and Settings.

The right rail title should be reduced or removed from the gameplay screen. The main menu already establishes the brand. During play, the UI should prioritize the current board state and the player's next action.

## Visual Direction

The game screen should keep the cozy tabletop identity but become cleaner and more intentional:

- The board remains warm, tactile, and high-contrast enough for the cat pieces.
- The HUD uses compact grouped information rather than large stacked cards.
- Action buttons use consistent game-style button treatment instead of plain desktop controls.
- The status area reserves stable space for one or two lines of guidance.
- Avoid nested cards, decorative blobs, and oversized headings.
- Keep all text readable at the current window size and avoid layout shifts when status text changes.

## Interaction Requirements

The following interactions must keep working exactly as before:

- Main menu Continue enters gameplay.
- Menu returns to the main menu without discarding the current game.
- New Game starts a new run from the gameplay screen.
- Settings opens the dedicated settings screen.
- Selecting cats, previewing reachable paths, moving cats, scoring, clearing, and game over still work.
- The game-over overlay remains above the whole playing screen and covers the content cleanly.

## Test Requirements

Update the WPF smoke tests to protect the new screen structure:

- `GameplayView` becomes visible after `ContinueCommand`.
- `GameplayShell`, `GameplayBoardArea`, `GameplayHudPanel`, `GameplayStatusBanner`, `GameplayScoreBlock`, `GameplayNextCatsBlock`, and `GameplayActionBar` exist.
- The status banner has a stable minimum height.
- The board frame still exists and preserves its current board rendering behavior.
- The gameplay action buttons are bound to the shell/game commands.
- Existing cat piece, path preview, score delta, clear feedback, rejected target, sound, and game-over overlay smoke tests continue to pass.

## Non-goals

This step does not change scoring, board size, line detection, save data, cat image assets, sound implementation, or the dedicated settings screen. It also does not add new gameplay mechanics.

## Acceptance Criteria

- The gameplay screen feels like a focused game scene rather than a desktop form.
- The board is still the visual anchor.
- The status prompt no longer changes layout height when selecting a cat.
- Score, best score, next cats, Menu, New Game, and Settings are still immediately discoverable.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

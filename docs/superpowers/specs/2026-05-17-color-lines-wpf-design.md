# Color Lines WPF Design

Date: 2026-05-17

## Goal

Build a polished Windows desktop version of Color Lines as a maintainable WPF game application. The first release focuses on the Windows experience, modern interaction, cute cat-themed pieces, and a theme structure that can grow over time.

The game should feel like a complete casual desktop game, not a throwaway demo. The player moves round cat-head pieces on a 9x9 board, clears lines of five or more matching cats, earns score, and plays until the board fills.

## Product Scope

The first version includes:

- A 9x9 board with classic Color Lines movement and clearing rules.
- Round cat-head pieces as the default visual identity.
- Seven visually distinct cat piece types, each still easy to recognize by color.
- Next-piece preview, score, high score, new game, pause, and settings entry points.
- Interaction feedback for selection, movement, spawning, clearing, scoring, invalid moves, and game over.
- A default board theme designed as the first member of an extensible theme system.
- Local persistence for high score, settings, window size, and the most recent game state.
- Windows desktop polish: resize behavior, high-DPI support, and mouse-first interaction.

The first version does not include accounts, networking, cloud saves, a store, complex level progression, or Web delivery. Future Web work may reuse rules and data concepts, but the first implementation optimizes for a strong Windows application.

## Technology Choice

Use .NET 8, C#, and WPF.

WPF is the preferred route because the game is a lightweight 2D board game where user experience, animation, resource styling, and maintainability matter more than heavy rendering. WPF gives a clear C# codebase, mature desktop layout, animation support, and a comfortable maintenance path for the project owner.

WinUI 3 is more modern on paper but carries more risk for this type of custom animated game surface. Unity or Godot would provide richer game tooling but add unnecessary complexity for the first Windows-focused release.

## Architecture

The solution should be split into clear projects:

```text
src/
  ColorLines.Core/
    Board/
    Rules/
    Scoring/
    Storage/
  ColorLines.Windows/
    Views/
    ViewModels/
    Controls/
    Themes/
    Assets/
    Services/
tests/
  ColorLines.Tests/
docs/
  superpowers/
    specs/
```

`ColorLines.Core` contains pure game rules and state. It must not depend on WPF or any Windows UI API. It owns the board model, piece types, pathfinding, move validation, line detection, scoring, spawning, game-over checks, and serializable state structures.

`ColorLines.Windows` contains the WPF application. It owns windows, views, view models, controls, theme resources, animations, sound playback, local file services, settings, and desktop-specific behavior.

`ColorLines.Tests` covers the core rules. Tests should focus on deterministic behavior and edge cases rather than UI animation.

## Runtime Flow

The WPF layer translates player clicks into intents. The core layer applies the intent and returns updated game state plus a list of domain events. The WPF layer renders the new state and plays animations based on those events.

Representative events:

```text
PieceSelected
MoveRejected
PieceMoved
LinesCleared
PiecesSpawned
ScoreChanged
GameOver
```

This event boundary keeps rules and presentation separate. Core decides what happened. Windows decides how it feels.

## Gameplay Rules

The board is 9x9. The game starts by spawning an initial set of cat pieces. On each turn, the player selects an existing piece and moves it to an empty reachable cell. Movement uses four-directional pathfinding and cannot pass through occupied cells.

After a valid move, the core checks horizontal, vertical, and both diagonal directions for runs of five or more matching pieces. If one or more lines are cleared, the player scores and no new pieces spawn for that turn. If no line is cleared, the next previewed batch of pieces is spawned into empty cells, and any lines created by spawning are then cleared.

The game ends when the board is full or the next spawn cannot be completed.

Scoring should be centralized in the core rules. The first scoring model should be simple and readable: a base score for five in a row, extra points for longer lines, and a multiplier for multiple lines cleared at once.

## UI Design

The main screen is the game. There is no marketing-style landing page. The board is the primary visual object, with score, high score, next pieces, new game, pause, and settings arranged in a compact status area.

On wide windows, the status area can sit to the right of the board. On narrow windows, it can move below the board. The board remains the priority and should keep stable square cells at all supported sizes.

The default theme is a warm modern board theme. It can use soft wood, fabric, or cozy tabletop cues, but the visual hierarchy must stay clear: cells, pieces, legal targets, and selected pieces should be readable at a glance.

## Cat Pieces

The default piece style is a round cat-head ball. Each piece is still recognizable as a Color Lines piece, but with a cute cat identity.

The seven default pieces should use different cat personalities and markings, such as orange cat, gray cat, tuxedo cat, calico cat, black cat, white cat, and blue-gray cat. Color readability matters. Pieces must not rely only on subtle markings.

The first implementation may use vector or procedural placeholder cat heads if final art is not ready. The code and resource structure should allow later replacement with final image assets without changing core game rules.

## Interaction And Animation

The first release should include animation for the main interaction moments:

- Selected cat: slight lift, bounce, or breathing ring.
- Hovered legal target: soft landing highlight.
- Invalid move: short shake or blocked feedback.
- Movement: quick path-based hop from cell to cell.
- Spawn: scale or pop-in animation with light landing feedback.
- Clear: line flash followed by star or yarn-like particle feedback.
- Score: smooth number increment and short combo cue when applicable.
- Game over: board dims and a lightweight end panel appears.

Animations should improve clarity and delight without slowing play. The player should be able to continue interacting at a comfortable pace.

## Theme And Asset System

Themes should be structured from the first version as replaceable packages of board visuals, piece visuals, effects, and sounds.

Example:

```text
Assets/
  Themes/
    CozyBoard/
      board/
      pieces/
      effects/
      sounds/
```

The default theme is `CozyBoard`. Future themes may change board material, cell styling, selected-piece glow, clear effects, piece art, and sounds. Theme loading belongs in the Windows layer, not the core.

Settings should include controls for theme choice, sound on/off, and animation intensity. The first version can ship with one complete theme while keeping the model ready for more.

## Persistence

Store local user data for:

- High score.
- Settings.
- Most recent game state.
- Window size and placement.

Persistence should use a simple local format, likely JSON, with version fields so future releases can migrate safely.

## Testing

Core tests should cover:

- Pathfinding on empty, blocked, and edge-case boards.
- Line detection in horizontal, vertical, and both diagonal directions.
- Runs longer than five.
- Multiple simultaneous clears.
- Turn behavior where clearing prevents spawn.
- Turn behavior where no clear triggers spawn.
- Spawning only into empty cells.
- Nearly full boards.
- Game-over behavior.
- State serialization and restore consistency.

UI tests are not required for the first design pass, but the application should be manually verified on Windows for resizing, selection, movement, animation timing, and persistence.

## Implementation Phases

### Phase 1: Project Skeleton And Core Rules

Create the .NET solution with `ColorLines.Core`, `ColorLines.Windows`, and `ColorLines.Tests`. Implement board state, pieces, pathfinding, move validation, clear detection, scoring, spawning, and game-over checks.

Acceptance: core tests pass and a simple state flow can run through a game turn.

### Phase 2: Basic WPF Playable Game

Implement the main window, 9x9 board, piece display, selection, movement, next preview, score, and new game. Use clean placeholder cat pieces.

Acceptance: the Windows app can play a complete game.

### Phase 3: Interaction Feel

Add selection, movement, spawn, clear, score, invalid move, and game-over animations.

Acceptance: major game events have clear visual feedback and the board remains stable while resizing.

### Phase 4: Theme And Resources

Organize the `CozyBoard` theme, establish resource naming, add theme-aware board and piece visuals, and implement settings for sound and animation intensity.

Acceptance: default theme is complete, and replacing resources does not require core rule changes.

### Phase 5: Local Desktop Polish

Add high score, settings persistence, recent game restore, window size memory, basic error handling, and maintainer-facing README instructions.

Acceptance: the app can be launched and used as a Windows desktop game, exits and restores user data, and is understandable for future maintenance.

## Final Acceptance Criteria

- The Windows application launches and can complete a full Color Lines game.
- The rules match classic Color Lines expectations.
- Cat pieces are the clear visual signature of the game.
- The default board theme feels complete and modern.
- Core rules are covered by focused tests.
- Rules, UI, theme resources, and persistence have clear ownership.
- The codebase is structured so the project owner can participate in maintenance.

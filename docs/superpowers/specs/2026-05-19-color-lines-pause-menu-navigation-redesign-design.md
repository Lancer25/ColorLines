# Color Lines Pause Menu Navigation Redesign Design

## Context

The gameplay HUD currently exposes direct actions such as New Game and Settings. That makes the running game feel like a desktop tool surface rather than a game screen. Common game menu patterns use a single in-game menu or pause entry during play, then place Resume, Save, Settings, End Game, and Return to Main Menu inside that menu.

The settings screen also needs source-aware navigation. If settings is opened from the main menu, Back should return to the main menu. If settings is opened from the in-game menu, Back should return to the in-game menu rather than jumping to the main menu.

## UX Direction

Use a single in-game menu entry from the gameplay HUD.

During active play, the HUD should keep the board, score, next cats, and one `Menu` action. Opening `Menu` shows an overlay-style pause menu with:

- Continue
- Save Game
- Settings
- End Game
- Return to Main Menu

The first implementation can make Save Game use the existing local save service path and show a short status message in the menu. End Game can be implemented as a clear command that starts a new game and returns to the main menu only if we add confirmation later; for this pass, Return to Main Menu is the non-destructive exit from the current run.

## Shell Model

Add a `PauseMenu` shell screen. Keep `MainMenu`, `Playing`, and `Settings`.

Add source-aware settings navigation:

- Main menu settings: `MainMenu -> Settings -> MainMenu`
- Pause menu settings: `PauseMenu -> Settings -> PauseMenu`

`BackToGameCommand` should return from pause menu to gameplay. `BackToMenuCommand` remains available for returning to the main menu from the pause menu or game-over modal.

## Gameplay HUD

Remove direct `New Game` and `Settings` actions from `GameplayActionBar`.

Keep only:

- `GameplayMenuButton`, bound to an `OpenPauseMenuCommand`

This makes gameplay feel cleaner and avoids accidental new-game starts during active play.

## Pause Menu View

Add a new `PauseMenuView` root-level screen/overlay with named regions:

- `PauseMenuView`
- `PauseMenuPanel`
- `PauseMenuActionList`
- `PauseContinueButton`
- `PauseSaveButton`
- `PauseSettingsButton`
- `PauseEndGameButton`
- `PauseBackToMenuButton`
- `PauseSaveStatusText`

Use existing menu button styles and cozy panel resources.

## Settings View

Replace direct `BackToMenuCommand` binding with a source-aware `CloseSettingsCommand`.

The settings screen should not know whether the user came from the main menu or pause menu through XAML state. It should bind to the shell command that chooses the correct return target.

## Performance Note

Separate from this navigation redesign, the current board can feel less smooth because `GameViewModel.RefreshFromState()` clears and recreates all 81 `CellViewModel` objects after selection, movement, and settings toggles. It also recomputes reachable paths for many cells during refresh. A later performance pass should move the board toward stable cell view models and incremental property updates.

## Test Requirements

Add or update tests to protect:

- Gameplay HUD has one menu action and no direct New Game or Settings buttons.
- Opening the gameplay menu shows `PauseMenuView`.
- Continue returns from pause menu to gameplay.
- Settings opened from pause menu returns to pause menu.
- Settings opened from main menu returns to main menu.
- Pause menu button commands bind to the shell commands.
- Existing game-over New Game and Return to Main Menu actions remain intact.

## Non-goals

This step does not implement confirmation dialogs, autosave slots, a separate save-load screen, or board rendering performance changes. It only fixes the navigation model and menu structure.

## Acceptance Criteria

- Gameplay HUD only exposes one in-game menu entry.
- Pause menu offers the expected game-style actions.
- Settings return behavior depends on where settings was opened.
- Existing main menu and game-over flows continue to work.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

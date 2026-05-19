# Color Lines Save Load Experience Design

## Context

The game already saves local data on window close and through the pause menu. Loading also works at startup. The remaining UX issue is that the player cannot clearly see whether Continue is resuming a saved game, when Save Game last ran, or what state will be restored.

## Recommendation

Add lightweight save/load visibility without changing the save file format.

The main menu should show a concise save summary derived from the loaded/current game state. The pause menu should show an explicit save confirmation when Save Game is pressed. Continue should remain the primary entry point, but nearby status text should make it feel like resuming the current run instead of starting an unknown state.

## UI Changes

Main menu:

- Add `MenuSaveSummary` in the existing status strip.
- Show score, best score, and whether a saved board exists.
- Keep Continue as the primary action.

Pause menu:

- Keep `PauseSaveStatusText`.
- Save status should include a simple confirmation such as `Game saved.`.
- Save command should remain non-disruptive and keep the pause menu open.

## View Model Changes

Expose derived text from `ShellViewModel` or `GameViewModel`:

- `SaveSummaryText`, based on current score, high score, and whether a game snapshot exists.
- Keep save/load file format unchanged.

## Test Requirements

Add or update tests to protect:

- Main menu includes `MenuSaveSummary`.
- `SaveSummaryText` reflects score and high score.
- Save command updates `PauseSaveStatusText`.
- Existing local save service tests continue to pass.

## Non-goals

This step does not add multiple save slots, load/delete save screens, timestamps in the save file, cloud sync, or confirmation dialogs.

## Acceptance Criteria

- Player can tell Continue resumes the current saved run.
- Pause menu save gives visible feedback.
- Save format remains backward compatible.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

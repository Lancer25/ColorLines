# Color Lines Incremental Board Refresh Design

## Context

Gameplay can feel less smooth because `GameViewModel.RefreshFromState()` clears and recreates all 81 `CellViewModel` objects after common interactions such as selecting a cat, moving a cat, starting a new game, and toggling animation. WPF then recreates item containers and reruns a large visual tree for each cell.

The same refresh also computes reachability by calling `PathFinder.FindPath` for many cells. That means a single selection can trigger many path searches before the player even chooses a target.

## Recommendation

Use stable cell view models with incremental property updates.

Create the 81 `CellViewModel` objects once and keep them in the same `ObservableCollection`. Refreshes should update each existing cell's properties instead of clearing the collection. This keeps WPF item containers stable, reduces visual tree churn, and gives animations/hover states a calmer base.

## Cell View Model Changes

`CellViewModel` should become a mutable notification object for cell state:

- Keep `Row` and `Column` immutable.
- Make occupancy, selection, piece metadata, feedback flags, reachable target, and path preview updateable properties.
- Add an `Update(...)` method that changes only values that differ and raises `PropertyChanged` per changed property.
- Preserve existing static factories for tests and readability, but have the game refresh path reuse existing instances.

## Game View Model Refresh

`GameViewModel` should initialize `Cells` once in board order.

`RefreshFromState()` should:

- Ensure `Cells` has 81 stable entries.
- Compute feedback flags from existing position sets.
- Update each existing `CellViewModel` instead of replacing it.
- Refresh `NextPieces` as before because it is only three items.

## Reachability Optimization

When no cat is selected, no cells are reachable.

When a cat is selected, compute reachable empty positions with one flood fill from the selected position instead of calling `PathFinder.FindPath` for each empty cell. Path preview can still use `PathFinder.FindPath` for the hovered target because it needs the actual path.

This should preserve behavior while reducing repeated work during selection refresh.

## Test Requirements

Add tests to protect:

- Selecting a cat keeps the same `CellViewModel` instances in `Cells`.
- Moving a cat keeps the same `CellViewModel` instances in `Cells`.
- New game keeps the collection stable while updating contents.
- Reachable target behavior remains the same.
- Path preview behavior remains the same.

Existing WPF smoke tests and gameplay tests must continue to pass.

## Non-goals

This step does not change rules, board size, scoring, save data, cat assets, or WPF layout. It also does not rewrite the board rendering template.

## Acceptance Criteria

- `Cells` is no longer cleared and rebuilt for normal refreshes.
- Selection, movement, path preview, scoring, clearing, and game over still behave the same.
- `dotnet test ColorLines.sln` passes.
- `dotnet build ColorLines.sln` passes.

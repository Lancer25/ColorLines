# Color Lines Phase 3 Interaction Feel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first layer of game-like interaction feedback to the playable WPF board.

**Architecture:** Keep animation decisions in the WPF layer while exposing lightweight event summary state from `GameViewModel`. XAML styles use WPF triggers and transitions for hover, selection, piece entry, clear feedback, score feedback, and game-over presentation.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Scope

This plan implements Phase 3 basics:

- View model exposes the last turn feedback: moved, spawned, cleared, rejected, game over, and score delta.
- Selected pieces get a stronger visual highlight.
- Cells with pieces use scale/opacity transforms for a simple pop-in feel.
- Rejected moves and scoring update status text clearly.
- Game over shows a lightweight overlay panel with a New Game button.
- WPF board styles become warmer and more game-like without introducing external assets.

This plan does not implement path-by-path animated movement, particle systems, sound, or full theme packaging. Those belong to later polish/theme phases.

## File Structure

Create or modify these files:

```text
src/ColorLines.Windows/ViewModels/TurnFeedback.cs
src/ColorLines.Windows/ViewModels/GameViewModel.cs
src/ColorLines.Windows/ViewModels/CellViewModel.cs
src/ColorLines.Windows/MainWindow.xaml
tests/ColorLines.Tests/GameViewModelTests.cs
```

## Task 1: Add Turn Feedback Model

**Files:**
- Create: `src/ColorLines.Windows/ViewModels/TurnFeedback.cs`
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests for feedback state**

Append these tests inside `GameViewModelTests`:

```csharp
[Fact]
public void NewGameStartsWithNeutralFeedback()
{
    var viewModel = GameViewModel.CreateForNewGame();

    Assert.False(viewModel.Feedback.HasScore);
    Assert.False(viewModel.Feedback.IsGameOver);
    Assert.Equal(0, viewModel.Feedback.ScoreDelta);
}

[Fact]
public void ClickingEmptyCellWithoutSelectionShowsRejectedFeedback()
{
    var viewModel = GameViewModel.CreateForNewGame();
    var empty = viewModel.Cells.First(cell => !cell.IsOccupied);

    viewModel.SelectCellCommand.Execute(empty);

    Assert.True(viewModel.Feedback.WasRejected);
    Assert.Contains("Select a cat", viewModel.StatusText);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `GameViewModel.Feedback` and `TurnFeedback` do not exist.

- [ ] **Step 3: Add `TurnFeedback`**

Create `src/ColorLines.Windows/ViewModels/TurnFeedback.cs`:

```csharp
namespace ColorLines.Windows.ViewModels;

public sealed record TurnFeedback(
    bool WasMoved,
    bool WasRejected,
    bool HadSpawn,
    bool HadClear,
    bool IsGameOver,
    int ScoreDelta)
{
    public static TurnFeedback Neutral { get; } = new(false, false, false, false, false, 0);

    public bool HasScore => ScoreDelta > 0;
}
```

- [ ] **Step 4: Update `GameViewModel` feedback behavior**

Modify `src/ColorLines.Windows/ViewModels/GameViewModel.cs`:

```csharp
private TurnFeedback feedback;
```

Initialize it in the constructor:

```csharp
feedback = TurnFeedback.Neutral;
```

Add the property:

```csharp
public TurnFeedback Feedback
{
    get => feedback;
    private set
    {
        if (feedback != value)
        {
            feedback = value;
            OnPropertyChanged();
        }
    }
}
```

In the branch where no cat is selected before clicking an empty cell, set:

```csharp
Feedback = new TurnFeedback(false, true, false, false, false, 0);
```

After `engine.Move(...)`, set:

```csharp
Feedback = BuildFeedback(result);
```

In `NewGame()`, set:

```csharp
Feedback = TurnFeedback.Neutral;
```

Add this helper:

```csharp
private static TurnFeedback BuildFeedback(GameTurnResult result)
{
    return new TurnFeedback(
        result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.PieceMoved),
        result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.MoveRejected),
        result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.PiecesSpawned),
        result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.LinesCleared),
        result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.GameOver),
        result.Events.LastOrDefault(gameEvent => gameEvent.Kind == GameEventKind.ScoreChanged)?.ScoreDelta ?? 0);
}
```

- [ ] **Step 5: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 6: Commit feedback model**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/TurnFeedback.cs src/ColorLines.Windows/ViewModels/GameViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: add turn feedback state"
```

Expected: commit succeeds.

## Task 2: Add Game Over And Status Presentation State

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests for presentation properties**

Append these tests inside `GameViewModelTests`:

```csharp
[Fact]
public void NewGameIsNotGameOver()
{
    var viewModel = GameViewModel.CreateForNewGame();

    Assert.False(viewModel.IsGameOver);
    Assert.Equal(string.Empty, viewModel.ScoreDeltaText);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `IsGameOver` and `ScoreDeltaText` do not exist.

- [ ] **Step 3: Add presentation properties**

In `src/ColorLines.Windows/ViewModels/GameViewModel.cs`, add:

```csharp
public bool IsGameOver => state.Status == GameStatus.GameOver || Feedback.IsGameOver;

public string ScoreDeltaText => Feedback.HasScore ? $"+{Feedback.ScoreDelta}" : string.Empty;
```

After changing `Feedback`, call:

```csharp
OnPropertyChanged(nameof(IsGameOver));
OnPropertyChanged(nameof(ScoreDeltaText));
```

After changing `state` in `NewGame()` and after moves, call:

```csharp
OnPropertyChanged(nameof(IsGameOver));
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 5: Commit presentation state**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/GameViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: expose game status presentation"
```

Expected: commit succeeds.

## Task 3: Improve Piece Visual Model

**Files:**
- Modify: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests for cat face text**

Append this test inside `GameViewModelTests`:

```csharp
[Fact]
public void CellViewModelProvidesCatFaceSymbol()
{
    var cell = CellViewModel.Occupied(1, 2, ColorLines.Core.Game.PieceKind.Orange, true);

    Assert.Equal("=^.^=", cell.FaceText);
    Assert.True(cell.IsSelected);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `FaceText` does not exist.

- [ ] **Step 3: Add `FaceText` to `CellViewModel`**

Change `CellViewModel` record signature to include `string FaceText` after `PieceName`. Update `Empty` to pass `string.Empty`. Update `Occupied` to pass `"=^.^="`.

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 5: Commit visual model**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/CellViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: add cat face presentation"
```

Expected: commit succeeds.

## Task 4: Add WPF Interaction Styling

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`

- [ ] **Step 1: Replace board cell visuals with animated cat piece styling**

Modify `src/ColorLines.Windows/MainWindow.xaml` so each board cell button contains:

```xml
<Grid Width="50" Height="50">
    <Ellipse x:Name="PieceBody"
             Fill="{Binding PieceBrush}"
             Stroke="#70452C"
             StrokeThickness="2"
             Opacity="0"
             RenderTransformOrigin="0.5,0.5">
        <Ellipse.RenderTransform>
            <ScaleTransform ScaleX="0.86" ScaleY="0.86" />
        </Ellipse.RenderTransform>
        <Ellipse.Style>
            <Style TargetType="Ellipse">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsOccupied}" Value="True">
                        <Setter Property="Opacity" Value="1" />
                        <DataTrigger.EnterActions>
                            <BeginStoryboard>
                                <Storyboard>
                                    <DoubleAnimation Storyboard.TargetProperty="RenderTransform.ScaleX"
                                                     To="1"
                                                     Duration="0:0:0.16" />
                                    <DoubleAnimation Storyboard.TargetProperty="RenderTransform.ScaleY"
                                                     To="1"
                                                     Duration="0:0:0.16" />
                                </Storyboard>
                            </BeginStoryboard>
                        </DataTrigger.EnterActions>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Ellipse.Style>
    </Ellipse>
    <TextBlock Text="{Binding FaceText}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"
               FontWeight="Bold"
               FontSize="11"
               Foreground="#FFF9F0" />
    <Border BorderBrush="#FFE06B"
            BorderThickness="3"
            CornerRadius="25"
            Opacity="0">
        <Border.Style>
            <Style TargetType="Border">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsSelected}" Value="True">
                        <Setter Property="Opacity" Value="1" />
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Border.Style>
    </Border>
</Grid>
```

- [ ] **Step 2: Add score delta and game-over overlay to `MainWindow.xaml`**

Add a `TextBlock` under score:

```xml
<TextBlock Text="{Binding ScoreDeltaText}"
           Foreground="#D96B2B"
           FontSize="18"
           FontWeight="Bold" />
```

Add a game-over overlay at the end of the root `Grid`:

```xml
<Border Grid.ColumnSpan="2"
        Background="#99000000"
        Visibility="{Binding IsGameOver, Converter={StaticResource BooleanToVisibilityConverter}}">
    <Border Width="320"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            Background="#FFFFFB"
            CornerRadius="14"
            Padding="24">
        <StackPanel>
            <TextBlock Text="Game Over"
                       FontSize="30"
                       FontWeight="Bold"
                       Foreground="#4A2D23"
                       HorizontalAlignment="Center" />
            <TextBlock Text="The board is full."
                       Margin="0,8,0,20"
                       FontSize="15"
                       Foreground="#8A6655"
                       HorizontalAlignment="Center" />
            <Button Content="New Game"
                    Command="{Binding NewGameCommand}"
                    Height="42"
                    Background="#7C4D2E"
                    Foreground="#FFFFFB"
                    BorderThickness="0"
                    FontSize="15"
                    FontWeight="SemiBold" />
        </StackPanel>
    </Border>
</Border>
```

Add this converter resource in `Window.Resources`:

```xml
<BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
```

- [ ] **Step 3: Build and test**

Run:

```powershell
dotnet test ColorLines.sln
dotnet build ColorLines.sln
```

Expected: all tests pass and build succeeds.

- [ ] **Step 4: Commit WPF interaction styling**

Run:

```powershell
git add src/ColorLines.Windows/MainWindow.xaml
git commit -m "feat: add wpf interaction feedback styling"
```

Expected: commit succeeds.

## Task 5: Phase 3 Verification

**Files:**
- Modify if needed: files changed by earlier tasks.

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test ColorLines.sln
```

Expected: all tests pass.

- [ ] **Step 2: Build the solution**

Run:

```powershell
dotnet build ColorLines.sln
```

Expected: build succeeds with zero errors.

- [ ] **Step 3: Launch the WPF app for smoke verification**

Run:

```powershell
dotnet run --project src/ColorLines.Windows/ColorLines.Windows.csproj
```

Expected: a Windows desktop app launches with animated round cat placeholders, a visible selection ring, score delta text after clears, and a game-over overlay when the board fills.

- [ ] **Step 4: Inspect git status**

Run:

```powershell
git status --short --branch
```

Expected: no uncommitted source changes.

## Self-Review

Spec coverage:

- Selection feedback: Tasks 1, 3, and 4.
- Spawn/pop-in feel: Task 4.
- Score feedback: Tasks 1, 2, and 4.
- Invalid move feedback: Task 1.
- Game-over panel: Tasks 2 and 4.
- Full movement path animation, particles, and sound are deferred because this phase is scoped to first-layer interaction feel.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

Type consistency:

- `TurnFeedback`, `GameViewModel.Feedback`, `IsGameOver`, `ScoreDeltaText`, and `CellViewModel.FaceText` are used consistently across tests and XAML.

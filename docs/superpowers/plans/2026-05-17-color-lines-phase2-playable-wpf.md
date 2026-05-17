# Color Lines Phase 2 Playable WPF Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the Phase 1 WPF shell into a basic playable Windows Color Lines game.

**Architecture:** Keep rules in `ColorLines.Core` and add a testable WPF view model layer in `ColorLines.Windows`. The WPF window binds to `GameViewModel`, which owns selection state, commands, board cell presentation, score display, next-piece display, and new-game flow.

**Tech Stack:** .NET 8, C#, WPF, xUnit.

---

## Scope

This plan implements Phase 2 from the design spec:

- 9x9 board displayed in WPF.
- Click occupied cell to select a cat piece.
- Click empty reachable cell to move the selected piece.
- Invalid movement keeps the board unchanged and reports a status message.
- Core engine handles clearing, spawning, scoring, and game over.
- Next-piece preview and score update after moves.
- New game command resets the board.
- Round cat-head placeholder pieces are rendered in WPF with color and short labels.

This plan does not implement Phase 3 animation, Phase 4 theme packages, or Phase 5 persistence.

## File Structure

Create or modify these files:

```text
src/ColorLines.Windows/ColorLines.Windows.csproj
src/ColorLines.Windows/MainWindow.xaml
src/ColorLines.Windows/MainWindow.xaml.cs
src/ColorLines.Windows/ViewModels/CellViewModel.cs
src/ColorLines.Windows/ViewModels/GameViewModel.cs
src/ColorLines.Windows/ViewModels/RelayCommand.cs
tests/ColorLines.Tests/ColorLines.Tests.csproj
tests/ColorLines.Tests/GameViewModelTests.cs
```

## Task 1: Make Windows ViewModels Testable

**Files:**
- Modify: `tests/ColorLines.Tests/ColorLines.Tests.csproj`
- Create: `src/ColorLines.Windows/ViewModels/RelayCommand.cs`
- Create: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Update the test project so it can reference the WPF project**

Replace `tests/ColorLines.Tests/ColorLines.Tests.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.5.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ColorLines.Core\ColorLines.Core.csproj" />
    <ProjectReference Include="..\..\src\ColorLines.Windows\ColorLines.Windows.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Write the first failing view model utility test**

Create `tests/ColorLines.Tests/GameViewModelTests.cs`:

```csharp
using ColorLines.Windows.ViewModels;

namespace ColorLines.Tests;

public sealed class GameViewModelTests
{
    [Fact]
    public void RelayCommandRunsExecuteWhenAllowed()
    {
        var executed = false;
        var command = new RelayCommand(_ => executed = true, _ => true);

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void RelayCommandDoesNotRunExecuteWhenBlocked()
    {
        var executed = false;
        var command = new RelayCommand(_ => executed = true, _ => false);

        Assert.False(command.CanExecute(null));

        Assert.False(executed);
    }
}
```

- [ ] **Step 3: Run the tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `ColorLines.Windows.ViewModels.RelayCommand` does not exist.

- [ ] **Step 4: Add `RelayCommand`**

Create `src/ColorLines.Windows/ViewModels/RelayCommand.cs`:

```csharp
using System.Windows.Input;

namespace ColorLines.Windows.ViewModels;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Predicate<object?>? canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            execute(parameter);
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

- [ ] **Step 5: Run the view model utility tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: both `RelayCommand` tests pass.

- [ ] **Step 6: Commit the testable Windows setup**

Run:

```powershell
git add tests/ColorLines.Tests/ColorLines.Tests.csproj tests/ColorLines.Tests/GameViewModelTests.cs src/ColorLines.Windows/ViewModels/RelayCommand.cs
git commit -m "test: prepare wpf view model tests"
```

Expected: commit succeeds.

## Task 2: Add Cell View Models

**Files:**
- Create: `src/ColorLines.Windows/ViewModels/CellViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests for cell presentation**

Append these tests inside `GameViewModelTests`:

```csharp
[Fact]
public void CellViewModelShowsEmptyCell()
{
    var cell = CellViewModel.Empty(4, 5);

    Assert.Equal(4, cell.Row);
    Assert.Equal(5, cell.Column);
    Assert.False(cell.IsOccupied);
    Assert.Equal(string.Empty, cell.PieceLabel);
}

[Fact]
public void CellViewModelShowsOccupiedCatPiece()
{
    var cell = CellViewModel.Occupied(1, 2, ColorLines.Core.Game.PieceKind.Calico, false);

    Assert.True(cell.IsOccupied);
    Assert.Equal("C", cell.PieceLabel);
    Assert.Equal("Calico", cell.PieceName);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `CellViewModel` does not exist.

- [ ] **Step 3: Add `CellViewModel`**

Create `src/ColorLines.Windows/ViewModels/CellViewModel.cs`:

```csharp
using System.Windows.Media;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record CellViewModel(
    int Row,
    int Column,
    bool IsOccupied,
    bool IsSelected,
    string PieceLabel,
    string PieceName,
    Brush PieceBrush)
{
    public static CellViewModel Empty(int row, int column)
    {
        return new CellViewModel(row, column, false, false, string.Empty, string.Empty, Brushes.Transparent);
    }

    public static CellViewModel Occupied(int row, int column, PieceKind piece, bool isSelected)
    {
        return new CellViewModel(row, column, true, isSelected, ToLabel(piece), piece.ToString(), ToBrush(piece));
    }

    private static string ToLabel(PieceKind piece)
    {
        return piece switch
        {
            PieceKind.Orange => "O",
            PieceKind.Gray => "G",
            PieceKind.Tuxedo => "T",
            PieceKind.Calico => "C",
            PieceKind.Black => "B",
            PieceKind.White => "W",
            PieceKind.BlueGray => "N",
            _ => string.Empty
        };
    }

    private static Brush ToBrush(PieceKind piece)
    {
        return piece switch
        {
            PieceKind.Orange => Brushes.Orange,
            PieceKind.Gray => Brushes.Gray,
            PieceKind.Tuxedo => Brushes.DarkSlateGray,
            PieceKind.Calico => Brushes.SandyBrown,
            PieceKind.Black => Brushes.Black,
            PieceKind.White => Brushes.WhiteSmoke,
            PieceKind.BlueGray => Brushes.SteelBlue,
            _ => Brushes.Transparent
        };
    }
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 5: Commit cell presentation**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/CellViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: add board cell presentation model"
```

Expected: commit succeeds.

## Task 3: Add Playable Game View Model

**Files:**
- Create: `src/ColorLines.Windows/ViewModels/GameViewModel.cs`
- Modify: `tests/ColorLines.Tests/GameViewModelTests.cs`

- [ ] **Step 1: Add failing tests for game interaction**

Append these tests inside `GameViewModelTests`:

```csharp
[Fact]
public void GameViewModelStartsWithBoardScoreAndNextPieces()
{
    var viewModel = GameViewModel.CreateForNewGame();

    Assert.Equal(81, viewModel.Cells.Count);
    Assert.Equal(0, viewModel.Score);
    Assert.Equal(3, viewModel.NextPieces.Count);
    Assert.Equal("Select a cat to move.", viewModel.StatusText);
}

[Fact]
public void SelectingOccupiedCellMarksItSelected()
{
    var viewModel = GameViewModel.CreateForNewGame();
    var occupied = viewModel.Cells.First(cell => cell.IsOccupied);

    viewModel.SelectCellCommand.Execute(occupied);

    var selected = viewModel.Cells.Single(cell => cell.IsSelected);
    Assert.Equal(occupied.Row, selected.Row);
    Assert.Equal(occupied.Column, selected.Column);
    Assert.Contains("Selected", viewModel.StatusText);
}

[Fact]
public void NewGameCommandResetsScoreAndSelection()
{
    var viewModel = GameViewModel.CreateForNewGame();
    var occupied = viewModel.Cells.First(cell => cell.IsOccupied);
    viewModel.SelectCellCommand.Execute(occupied);

    viewModel.NewGameCommand.Execute(null);

    Assert.Equal(0, viewModel.Score);
    Assert.DoesNotContain(viewModel.Cells, cell => cell.IsSelected);
    Assert.Equal("Select a cat to move.", viewModel.StatusText);
}
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: compile fails because `GameViewModel` does not exist.

- [ ] **Step 3: Add `GameViewModel`**

Create `src/ColorLines.Windows/ViewModels/GameViewModel.cs`:

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;

namespace ColorLines.Windows.ViewModels;

public sealed class GameViewModel : INotifyPropertyChanged
{
    private readonly GameEngine engine;
    private GameState state;
    private BoardPosition? selectedPosition;
    private int score;
    private string statusText;

    public GameViewModel(GameEngine engine, GameState state)
    {
        this.engine = engine;
        this.state = state;
        score = state.Score;
        statusText = "Select a cat to move.";
        Cells = new ObservableCollection<CellViewModel>();
        NextPieces = new ObservableCollection<string>();
        SelectCellCommand = new RelayCommand(SelectCell, parameter => parameter is CellViewModel);
        NewGameCommand = new RelayCommand(_ => NewGame());
        RefreshFromState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CellViewModel> Cells { get; }

    public ObservableCollection<string> NextPieces { get; }

    public ICommand SelectCellCommand { get; }

    public ICommand NewGameCommand { get; }

    public int Score
    {
        get => score;
        private set
        {
            if (score != value)
            {
                score = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set
        {
            if (statusText != value)
            {
                statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public static GameViewModel CreateForNewGame()
    {
        var engine = new GameEngine(new SystemRandomSource());
        return new GameViewModel(engine, engine.NewGame());
    }

    private void SelectCell(object? parameter)
    {
        if (parameter is not CellViewModel cell)
        {
            return;
        }

        var position = new BoardPosition(cell.Row, cell.Column);
        var piece = state.Board.GetPiece(position);

        if (piece is not null)
        {
            selectedPosition = position;
            StatusText = $"Selected {piece}. Choose an empty cell.";
            RefreshFromState();
            return;
        }

        if (selectedPosition is null)
        {
            StatusText = "Select a cat before choosing a target.";
            return;
        }

        var result = engine.Move(state, selectedPosition.Value, position);
        state = result.State;
        selectedPosition = null;
        Score = state.Score;
        StatusText = BuildStatusText(result);
        RefreshFromState();
    }

    private void NewGame()
    {
        state = engine.NewGame();
        selectedPosition = null;
        Score = state.Score;
        StatusText = "Select a cat to move.";
        RefreshFromState();
    }

    private void RefreshFromState()
    {
        Cells.Clear();
        foreach (var cell in state.Board.Cells())
        {
            var isSelected = selectedPosition == cell.Position;
            Cells.Add(cell.Piece is null
                ? CellViewModel.Empty(cell.Position.Row, cell.Position.Column)
                : CellViewModel.Occupied(cell.Position.Row, cell.Position.Column, cell.Piece.Value, isSelected));
        }

        NextPieces.Clear();
        foreach (var piece in state.NextPieces)
        {
            NextPieces.Add(piece.ToString());
        }
    }

    private static string BuildStatusText(GameTurnResult result)
    {
        if (result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.MoveRejected))
        {
            return "That cat cannot move there.";
        }

        if (result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.GameOver))
        {
            return "Game over. Start a new game?";
        }

        var scoreEvent = result.Events.LastOrDefault(gameEvent => gameEvent.Kind == GameEventKind.ScoreChanged);
        if (scoreEvent is not null && scoreEvent.ScoreDelta > 0)
        {
            return $"+{scoreEvent.ScoreDelta} points!";
        }

        return "Select a cat to move.";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test tests/ColorLines.Tests/ColorLines.Tests.csproj --filter GameViewModelTests
```

Expected: all `GameViewModelTests` pass.

- [ ] **Step 5: Commit game view model**

Run:

```powershell
git add src/ColorLines.Windows/ViewModels/GameViewModel.cs tests/ColorLines.Tests/GameViewModelTests.cs
git commit -m "feat: add playable game view model"
```

Expected: commit succeeds.

## Task 4: Bind Main Window To The Playable View Model

**Files:**
- Modify: `src/ColorLines.Windows/MainWindow.xaml`
- Modify: `src/ColorLines.Windows/MainWindow.xaml.cs`

- [ ] **Step 1: Replace `MainWindow.xaml`**

Replace `src/ColorLines.Windows/MainWindow.xaml` with:

```xml
<Window x:Class="ColorLines.Windows.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Color Lines"
        Width="1060"
        Height="760"
        MinWidth="820"
        MinHeight="620"
        Background="#FFF7EF">
    <Window.Resources>
        <Style x:Key="BoardCellButton" TargetType="Button">
            <Setter Property="Margin" Value="4" />
            <Setter Property="Padding" Value="0" />
            <Setter Property="Background" Value="#FFF8E7" />
            <Setter Property="BorderBrush" Value="#E8B982" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="Cursor" Value="Hand" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border x:Name="CellFrame"
                                Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="10">
                            <ContentPresenter HorizontalAlignment="Center"
                                              VerticalAlignment="Center" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="CellFrame" Property="Background" Value="#FFF0CF" />
                                <Setter TargetName="CellFrame" Property="BorderBrush" Value="#D98B4A" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>

    <Grid Margin="28">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="300" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0"
                Background="#FFE1BB"
                BorderBrush="#D79B63"
                BorderThickness="2"
                CornerRadius="16"
                Padding="20">
            <Viewbox Stretch="Uniform">
                <ItemsControl ItemsSource="{Binding Cells}" Width="560" Height="560">
                    <ItemsControl.ItemsPanel>
                        <ItemsPanelTemplate>
                            <UniformGrid Rows="9" Columns="9" />
                        </ItemsPanelTemplate>
                    </ItemsControl.ItemsPanel>
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Button Style="{StaticResource BoardCellButton}"
                                    Command="{Binding DataContext.SelectCellCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                    CommandParameter="{Binding}">
                                <Grid Width="48" Height="48">
                                    <Ellipse Fill="{Binding PieceBrush}"
                                             Stroke="#70452C"
                                             StrokeThickness="2"
                                             Opacity="0">
                                        <Ellipse.Style>
                                            <Style TargetType="Ellipse">
                                                <Style.Triggers>
                                                    <DataTrigger Binding="{Binding IsOccupied}" Value="True">
                                                        <Setter Property="Opacity" Value="1" />
                                                    </DataTrigger>
                                                </Style.Triggers>
                                            </Style>
                                        </Ellipse.Style>
                                    </Ellipse>
                                    <TextBlock Text="{Binding PieceLabel}"
                                               HorizontalAlignment="Center"
                                               VerticalAlignment="Center"
                                               FontWeight="Bold"
                                               FontSize="18"
                                               Foreground="#FFF9F0" />
                                    <Border BorderBrush="#FFE06B"
                                            BorderThickness="3"
                                            CornerRadius="24"
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
                            </Button>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </Viewbox>
        </Border>

        <StackPanel Grid.Column="1" Margin="28,0,0,0">
            <TextBlock Text="Color Lines"
                       FontSize="34"
                       FontWeight="Bold"
                       Foreground="#4A2D23" />
            <TextBlock Text="{Binding StatusText}"
                       Margin="0,6,0,26"
                       TextWrapping="Wrap"
                       FontSize="15"
                       Foreground="#8A6655" />

            <Border Background="#FFFFFB"
                    BorderBrush="#E8CFB7"
                    BorderThickness="1"
                    CornerRadius="10"
                    Padding="18"
                    Margin="0,0,0,14">
                <StackPanel>
                    <TextBlock Text="Score"
                               Foreground="#8A6655"
                               FontSize="13"
                               FontWeight="SemiBold" />
                    <TextBlock Text="{Binding Score}"
                               Foreground="#4A2D23"
                               FontSize="34"
                               FontWeight="Bold" />
                </StackPanel>
            </Border>

            <Border Background="#FFFFFB"
                    BorderBrush="#E8CFB7"
                    BorderThickness="1"
                    CornerRadius="10"
                    Padding="18"
                    Margin="0,0,0,14">
                <StackPanel>
                    <TextBlock Text="Next Cats"
                               Foreground="#8A6655"
                               FontSize="13"
                               FontWeight="SemiBold" />
                    <ItemsControl ItemsSource="{Binding NextPieces}" Margin="0,10,0,0">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <StackPanel Orientation="Horizontal" />
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="#FFF0CF"
                                        CornerRadius="14"
                                        Padding="10,6"
                                        Margin="0,0,8,0">
                                    <TextBlock Text="{Binding}"
                                               Foreground="#4A2D23"
                                               FontSize="13"
                                               FontWeight="SemiBold" />
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </StackPanel>
            </Border>

            <Button Content="New Game"
                    Command="{Binding NewGameCommand}"
                    Height="42"
                    Background="#7C4D2E"
                    Foreground="#FFFFFB"
                    BorderThickness="0"
                    FontSize="15"
                    FontWeight="SemiBold" />
        </StackPanel>
    </Grid>
</Window>
```

- [ ] **Step 2: Replace `MainWindow.xaml.cs`**

Replace `src/ColorLines.Windows/MainWindow.xaml.cs` with:

```csharp
using System.Windows;
using ColorLines.Windows.ViewModels;

namespace ColorLines.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = GameViewModel.CreateForNewGame();
    }
}
```

- [ ] **Step 3: Build and test**

Run:

```powershell
dotnet test ColorLines.sln
dotnet build ColorLines.sln
```

Expected: all tests pass and build succeeds.

- [ ] **Step 4: Commit playable WPF window**

Run:

```powershell
git add src/ColorLines.Windows/MainWindow.xaml src/ColorLines.Windows/MainWindow.xaml.cs
git commit -m "feat: bind playable wpf board"
```

Expected: commit succeeds.

## Task 5: Phase 2 Verification

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

Expected: a Windows desktop app launches with a 9x9 board, score panel, next-piece preview, and New Game button.

- [ ] **Step 4: Inspect git status**

Run:

```powershell
git status --short --branch
```

Expected: no uncommitted source changes.

## Self-Review

Spec coverage:

- Basic WPF playable game: Tasks 3 and 4.
- 9x9 board and piece display: Task 4.
- Selection and movement: Task 3.
- Next preview, score, and new game: Tasks 3 and 4.
- Placeholder round cat pieces: Task 4.
- Animation, full theme packaging, persistence, and high score are deferred to later phase plans.

Placeholder scan:

- The plan does not use unresolved placeholder markers.

Type consistency:

- `GameViewModel`, `CellViewModel`, and `RelayCommand` use consistent namespaces and are referenced from both tests and WPF XAML/code-behind.

using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;
using ColorLines.Windows.ViewModels;
using ColorLines.Windows.Services;

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

    [Fact]
    public void GameViewModelStartsWithBoardScoreAndNextPieces()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        Assert.Equal(81, viewModel.Cells.Count);
        Assert.Equal(0, viewModel.Score);
        Assert.Equal(3, viewModel.NextPieces.Count);
        Assert.All(viewModel.NextPieces, piece => Assert.Equal("=^.^=", piece.FaceText));
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
    public void SelectingOccupiedCellKeepsCellInstancesStable()
    {
        var viewModel = GameViewModel.CreateForNewGame();
        var originalCells = viewModel.Cells.ToArray();
        var occupied = viewModel.Cells.First(cell => cell.IsOccupied);

        viewModel.SelectCellCommand.Execute(occupied);

        Assert.Equal(originalCells, viewModel.Cells);
    }

    [Fact]
    public void SelectingOccupiedCellPlaysSelectSound()
    {
        var soundPlayer = new RecordingSoundPlayer();
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state, soundPlayer: soundPlayer);
        var occupied = viewModel.Cells.Single(cell => cell.IsOccupied);

        viewModel.SelectCellCommand.Execute(occupied);

        Assert.Equal(new[] { SoundCue.Select }, soundPlayer.Cues);
    }

    [Fact]
    public void DisabledSoundDoesNotPlaySelectCue()
    {
        var soundPlayer = new RecordingSoundPlayer();
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state, soundPlayer: soundPlayer);
        var occupied = viewModel.Cells.Single(cell => cell.IsOccupied);
        viewModel.ToggleSoundCommand.Execute(null);

        viewModel.SelectCellCommand.Execute(occupied);

        Assert.Empty(soundPlayer.Cues);
    }

    [Fact]
    public void SelectingOccupiedCellMarksReachableEmptyTargets()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(8, 8), PieceKind.Gray);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);

        viewModel.SelectCellCommand.Execute(source);

        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsReachableTarget);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.IsOccupied && cell.IsReachableTarget);
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

    [Fact]
    public void NewGameCommandKeepsCellInstancesStable()
    {
        var viewModel = GameViewModel.CreateForNewGame();
        var originalCells = viewModel.Cells.ToArray();

        viewModel.NewGameCommand.Execute(null);

        Assert.Equal(originalCells, viewModel.Cells);
    }

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
        Assert.Contains(viewModel.Cells, cell => cell.Row == empty.Row && cell.Column == empty.Column && cell.WasRejectedTarget);
        Assert.Contains("Select a cat", viewModel.StatusText);
    }

    [Fact]
    public void ReducedAnimationSuppressesRejectFeedback()
    {
        var viewModel = GameViewModel.CreateForNewGame();
        viewModel.ToggleAnimationCommand.Execute(null);
        var empty = viewModel.Cells.First(cell => !cell.IsOccupied);

        viewModel.SelectCellCommand.Execute(empty);

        Assert.True(viewModel.Feedback.WasRejected);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.WasRejectedTarget);
    }

    [Fact]
    public void ClickingEmptyCellWithoutSelectionPlaysRejectSound()
    {
        var soundPlayer = new RecordingSoundPlayer();
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), new GameEngine(new SequenceRandomSource()).NewGame(), soundPlayer: soundPlayer);
        var empty = viewModel.Cells.First(cell => !cell.IsOccupied);

        viewModel.SelectCellCommand.Execute(empty);

        Assert.Equal(new[] { SoundCue.Reject }, soundPlayer.Cues);
    }

    [Fact]
    public void SuccessfulMoveMarksTargetCellAsMovedTo()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 1);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.True(viewModel.Feedback.WasMoved);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.WasMovedTo);
    }

    [Fact]
    public void SuccessfulMoveKeepsCellInstancesStable()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var originalCells = viewModel.Cells.ToArray();
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 1);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.Equal(originalCells, viewModel.Cells);
    }

    [Fact]
    public void SuccessfulMovePlaysMoveSound()
    {
        var soundPlayer = new RecordingSoundPlayer();
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state, soundPlayer: soundPlayer);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 1);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.Equal(new[] { SoundCue.Select, SoundCue.Move }, soundPlayer.Cues);
    }

    [Fact]
    public void SuccessfulMoveMarksPathCells()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 0 && cell.WasMovePath);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.WasMovePath);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 2 && cell.WasMovePath);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 2 && cell.WasMovedTo);
    }

    [Fact]
    public void ReducedAnimationSuppressesMoveFeedback()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);

        viewModel.ToggleAnimationCommand.Execute(null);
        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.True(viewModel.Feedback.WasMoved);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.WasMovedTo);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.WasMovePath);
    }

    [Fact]
    public void SelectingOccupiedCellClearsMovePathFeedback()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(8, 8), PieceKind.Gray);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);
        var nextSource = viewModel.Cells.Single(cell => cell.Row == 8 && cell.Column == 8);
        viewModel.SelectCellCommand.Execute(nextSource);

        Assert.DoesNotContain(viewModel.Cells, cell => cell.WasMovePath);
    }

    [Fact]
    public void PreviewPathCommandMarksPathCellsForReachableTarget()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        viewModel.SelectCellCommand.Execute(source);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);

        viewModel.PreviewPathCommand.Execute(target);

        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 0 && cell.IsPathPreview);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsPathPreview);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 2 && cell.IsPathPreview);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 2 && cell.IsPathPreviewTarget);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsPathPreviewTarget);
    }

    [Fact]
    public void ReducedAnimationKeepsPlanningFeedback()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        viewModel.ToggleAnimationCommand.Execute(null);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);

        viewModel.SelectCellCommand.Execute(source);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);
        viewModel.PreviewPathCommand.Execute(target);

        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsReachableTarget);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsPathPreview);
    }

    [Fact]
    public void PreviewPathCommandDoesNotRefreshBoardWhenNoPathCanBeShown()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var occupied = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var collectionChanges = 0;
        viewModel.Cells.CollectionChanged += (_, _) => collectionChanges++;

        viewModel.PreviewPathCommand.Execute(occupied);

        Assert.Equal(0, collectionChanges);
    }

    [Fact]
    public void PreviewPathCommandDoesNotRebuildBoardForReachableTarget()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        viewModel.SelectCellCommand.Execute(source);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);
        var collectionChanges = 0;
        viewModel.Cells.CollectionChanged += (_, _) => collectionChanges++;

        viewModel.PreviewPathCommand.Execute(target);

        Assert.Equal(0, collectionChanges);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 1 && cell.IsPathPreview);
    }

    [Fact]
    public void ClearPreviewPathCommandRemovesPathWithoutClearingSelection()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        viewModel.SelectCellCommand.Execute(source);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 2);
        viewModel.PreviewPathCommand.Execute(target);

        viewModel.ClearPreviewPathCommand.Execute(null);

        Assert.DoesNotContain(viewModel.Cells, cell => cell.IsPathPreview);
        Assert.DoesNotContain(viewModel.Cells, cell => cell.IsPathPreviewTarget);
        Assert.Contains(viewModel.Cells, cell => cell.Row == 0 && cell.Column == 0 && cell.IsSelected);
    }

    [Fact]
    public void NewGameIsNotGameOver()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        Assert.False(viewModel.IsGameOver);
        Assert.Equal(string.Empty, viewModel.ScoreDeltaText);
    }

    [Fact]
    public void GameOverViewModelProvidesSummaryText()
    {
        var state = new GameState(GameBoard.CreateEmpty(), Array.Empty<PieceKind>(), 42, GameStatus.GameOver);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state, highScore: 99);

        Assert.True(viewModel.IsGameOver);
        Assert.Equal("Game Over", viewModel.GameOverTitle);
        Assert.Equal("The board is full. Start a new run?", viewModel.GameOverSummaryText);
        Assert.Equal("Final Score: 42", viewModel.FinalScoreText);
        Assert.Equal("Best Score: 99", viewModel.BestScoreText);
    }

    [Fact]
    public void ScoringMoveShowsScoreDeltaBadge()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 2), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 3), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 4), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 5), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 1);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);

        Assert.True(viewModel.ShowScoreDelta);
        Assert.Equal(viewModel.ScoreDeltaText, viewModel.ScoreDeltaBadgeText);
        Assert.StartsWith("+", viewModel.ScoreDeltaBadgeText);
    }

    [Fact]
    public void SelectingOccupiedCellClearsScoreDeltaBadge()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 2), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 3), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 4), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 5), PieceKind.Orange);
        board.SetPiece(new BoardPosition(8, 8), PieceKind.Gray);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var viewModel = new GameViewModel(new GameEngine(new SequenceRandomSource()), state);
        var source = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 0);
        var target = viewModel.Cells.Single(cell => cell.Row == 0 && cell.Column == 1);

        viewModel.SelectCellCommand.Execute(source);
        viewModel.SelectCellCommand.Execute(target);
        var nextSource = viewModel.Cells.Single(cell => cell.Row == 8 && cell.Column == 8);
        viewModel.SelectCellCommand.Execute(nextSource);

        Assert.False(viewModel.ShowScoreDelta);
        Assert.Equal(string.Empty, viewModel.ScoreDeltaBadgeText);
    }

    [Fact]
    public void NewGameHidesScoreDeltaBadge()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        Assert.False(viewModel.ShowScoreDelta);
        Assert.Equal(string.Empty, viewModel.ScoreDeltaBadgeText);
    }

    [Fact]
    public void CellViewModelProvidesCatFaceSymbol()
    {
        var cell = CellViewModel.Occupied(1, 2, ColorLines.Core.Game.PieceKind.Orange, true);

        Assert.NotNull(cell.Piece);
        Assert.Equal("=^.^=", cell.Piece.FaceText);
        Assert.True(cell.IsSelected);
    }

    [Fact]
    public void PieceViewModelUsesReadableCatColors()
    {
        var white = PieceViewModel.FromPiece(ColorLines.Core.Game.PieceKind.White);
        var black = PieceViewModel.FromPiece(ColorLines.Core.Game.PieceKind.Black);

        Assert.Equal("White", white.Name);
        Assert.Equal("W", white.Label);
        Assert.Equal("=^.^=", white.FaceText);
        Assert.NotEqual(white.FaceBrush.ToString(), black.FaceBrush.ToString());
    }

    [Fact]
    public void PieceViewModelProvidesAvatarAccentBrushes()
    {
        var orange = PieceViewModel.FromPiece(PieceKind.Orange);

        Assert.NotNull(orange.HighlightBrush);
        Assert.NotNull(orange.ShadowBrush);
        Assert.NotNull(orange.InnerEarBrush);
        Assert.NotEqual(orange.BodyBrush.ToString(), orange.InnerEarBrush.ToString());
    }

    [Fact]
    public void PieceViewModelProvidesThemeAssetPath()
    {
        var orange = PieceViewModel.FromPiece(PieceKind.Orange);
        var blueGray = PieceViewModel.FromPiece(PieceKind.BlueGray);

        Assert.Equal("/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/orange.png", orange.AssetPath);
        Assert.Equal("/ColorLines.Windows;component/Assets/Themes/CozyBoard/pieces/bluegray.png", blueGray.AssetPath);
    }

    [Fact]
    public void GameViewModelExposesDefaultThemeAndSettings()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        Assert.Equal("Cozy Board", viewModel.SelectedThemeName);
        Assert.True(viewModel.IsSoundEnabled);
        Assert.Equal("Full", viewModel.AnimationIntensity);
        Assert.True(viewModel.IsFullAnimation);
        Assert.Equal("Use Reduced Animation", viewModel.AnimationToggleText);
    }

    [Fact]
    public void ToggleSoundSwitchesSoundSetting()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        viewModel.ToggleSoundCommand.Execute(null);

        Assert.False(viewModel.IsSoundEnabled);
    }

    [Fact]
    public void ToggleAnimationSwitchesAnimationIntensity()
    {
        var viewModel = GameViewModel.CreateForNewGame();

        viewModel.ToggleAnimationCommand.Execute(null);

        Assert.Equal("Reduced", viewModel.AnimationIntensity);
        Assert.False(viewModel.IsFullAnimation);
        Assert.Equal("Use Full Animation", viewModel.AnimationToggleText);

        viewModel.ToggleAnimationCommand.Execute(null);

        Assert.Equal("Full", viewModel.AnimationIntensity);
        Assert.True(viewModel.IsFullAnimation);
        Assert.Equal("Use Reduced Animation", viewModel.AnimationToggleText);
    }

    [Fact]
    public void CreateFromSaveRestoresHighScoreAndSettings()
    {
        var save = new LocalSaveData(1, 99, false, "Reduced", "CozyBoard", null, new WindowPlacementData(800, 600));

        var viewModel = GameViewModel.CreateFromSave(save);

        Assert.Equal(99, viewModel.HighScore);
        Assert.False(viewModel.IsSoundEnabled);
        Assert.Equal("Reduced", viewModel.AnimationIntensity);
        Assert.False(viewModel.IsFullAnimation);
    }

    [Fact]
    public void CreateSaveDataExportsCurrentSettings()
    {
        var viewModel = GameViewModel.CreateForNewGame();
        viewModel.ToggleSoundCommand.Execute(null);

        var save = viewModel.CreateSaveData(new WindowPlacementData(1000, 720));

        Assert.False(save.IsSoundEnabled);
        Assert.Equal("Full", save.AnimationIntensity);
        Assert.Equal("CozyBoard", save.ThemeId);
        Assert.NotNull(save.Game);
    }

    [Fact]
    public void CreateSaveDataExportsAnimationIntensity()
    {
        var viewModel = GameViewModel.CreateForNewGame();
        viewModel.ToggleAnimationCommand.Execute(null);

        var save = viewModel.CreateSaveData(new WindowPlacementData(1000, 720));

        Assert.Equal("Reduced", save.AnimationIntensity);
    }

    [Fact]
    public void ShellViewModelStartsAtMainMenu()
    {
        var game = GameViewModel.CreateForNewGame();
        var shell = new ShellViewModel(game);

        Assert.Equal(ShellScreen.MainMenu, shell.CurrentScreen);
        Assert.True(shell.IsMainMenuVisible);
        Assert.False(shell.IsPlayingVisible);
        Assert.False(shell.IsSettingsVisible);
        Assert.Same(game, shell.Game);
    }

    [Fact]
    public void ContinueCommandOpensPlayingScreen()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);

        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
        Assert.False(shell.IsMainMenuVisible);
        Assert.True(shell.IsPlayingVisible);
    }

    [Fact]
    public void SettingsCommandOpensSettingsScreenAndCloseReturnsToMenu()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.OpenSettingsCommand.Execute(null);

        Assert.Equal(ShellScreen.Settings, shell.CurrentScreen);
        Assert.True(shell.IsSettingsVisible);

        shell.CloseSettingsCommand.Execute(null);

        Assert.Equal(ShellScreen.MainMenu, shell.CurrentScreen);
    }

    [Fact]
    public void PauseMenuCommandOpensPauseMenuAndContinueReturnsToGame()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);

        shell.BackToGameCommand.Execute(null);

        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
        Assert.True(shell.IsPlayingVisible);
    }

    [Fact]
    public void SettingsOpenedFromPauseMenuReturnsToPauseMenu()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);
        shell.OpenSettingsCommand.Execute(null);

        Assert.Equal(ShellScreen.Settings, shell.CurrentScreen);

        shell.CloseSettingsCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);
    }

    [Fact]
    public void SaveGameCommandRaisesSaveRequestedAndUpdatesStatus()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());
        var raised = false;
        shell.SaveRequested += (_, _) => raised = true;

        shell.SaveGameCommand.Execute(null);

        Assert.True(raised);
        Assert.Equal("Game saved.", shell.PauseSaveStatusText);
    }

    [Fact]
    public void SaveSummaryTextDescribesCurrentRun()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        Assert.True(shell.CanContinueGame);
        Assert.True(shell.ContinueCommand.CanExecute(null));
        Assert.Equal("Continue available: Score 0 | Best 0", shell.SaveSummaryText);
    }

    [Fact]
    public void SaveSummaryTextUpdatesWhenScoreChanges()
    {
        var board = GameBoard.CreateEmpty();
        for (var column = 0; column < 4; column++)
        {
            board.SetPiece(new BoardPosition(0, column), PieceKind.Orange);
        }

        board.SetPiece(new BoardPosition(1, 0), PieceKind.Orange);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var shell = new ShellViewModel(new GameViewModel(new GameEngine(new SequenceRandomSource()), state));
        var source = shell.Game.Cells.Single(cell => cell.Row == 1 && cell.Column == 0);
        var target = shell.Game.Cells.Single(cell => cell.Row == 0 && cell.Column == 4);

        shell.Game.SelectCellCommand.Execute(source);
        shell.Game.SelectCellCommand.Execute(target);

        Assert.Equal("Continue available: Score 10 | Best 10", shell.SaveSummaryText);
    }

    [Fact]
    public void ContinueCommandIsDisabledAfterGameOver()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);
        shell.EndGameCommand.Execute(null);
        shell.ConfirmEndGameCommand.Execute(null);
        shell.BackToMenuCommand.Execute(null);

        Assert.False(shell.CanContinueGame);
        Assert.False(shell.ContinueCommand.CanExecute(null));
        Assert.Equal("Start a new game: Score 0 | Best 0", shell.SaveSummaryText);
    }

    [Fact]
    public void NewGameCommandRestoresContinueAvailability()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);
        shell.EndGameCommand.Execute(null);
        shell.ConfirmEndGameCommand.Execute(null);

        Assert.False(shell.CanContinueGame);

        shell.NewGameCommand.Execute(null);

        Assert.True(shell.CanContinueGame);
        Assert.True(shell.ContinueCommand.CanExecute(null));
        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
    }

    [Fact]
    public void PauseMenuReturnToMainMenuRequiresConfirmation()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);
        shell.RequestBackToMenuCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);
        Assert.True(shell.IsReturnToMenuConfirmVisible);

        shell.CancelBackToMenuCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);
        Assert.False(shell.IsReturnToMenuConfirmVisible);

        shell.RequestBackToMenuCommand.Execute(null);
        shell.ConfirmBackToMenuCommand.Execute(null);

        Assert.Equal(ShellScreen.MainMenu, shell.CurrentScreen);
        Assert.True(shell.IsMainMenuVisible);
        Assert.False(shell.IsReturnToMenuConfirmVisible);
    }

    [Fact]
    public void EndGameRequiresConfirmationBeforeEndingRun()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.OpenPauseMenuCommand.Execute(null);
        shell.EndGameCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);
        Assert.True(shell.IsEndGameConfirmVisible);
        Assert.False(shell.IsReturnToMenuConfirmVisible);
        Assert.False(shell.Game.IsGameOver);

        shell.CancelEndGameCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.True(shell.IsPauseMenuVisible);
        Assert.False(shell.IsEndGameConfirmVisible);
        Assert.False(shell.Game.IsGameOver);

        shell.EndGameCommand.Execute(null);
        shell.ConfirmEndGameCommand.Execute(null);

        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
        Assert.True(shell.IsPlayingVisible);
        Assert.True(shell.Game.IsGameOver);
        Assert.Equal("Game over. Start a new game?", shell.Game.StatusText);
        Assert.False(shell.IsEndGameConfirmVisible);
        Assert.False(shell.IsReturnToMenuConfirmVisible);
    }

    [Fact]
    public void EscapeCommandNavigatesPauseFlowSafely()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());

        shell.ContinueCommand.Execute(null);
        shell.EscapeCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);

        shell.EndGameCommand.Execute(null);
        shell.EscapeCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.False(shell.IsEndGameConfirmVisible);

        shell.RequestBackToMenuCommand.Execute(null);
        shell.EscapeCommand.Execute(null);

        Assert.Equal(ShellScreen.PauseMenu, shell.CurrentScreen);
        Assert.False(shell.IsReturnToMenuConfirmVisible);

        shell.EscapeCommand.Execute(null);

        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
    }

    [Fact]
    public void NewGameCommandResetsGameAndOpensPlayingScreen()
    {
        var shell = new ShellViewModel(GameViewModel.CreateForNewGame());
        var originalGame = shell.Game;
        var occupied = originalGame.Cells.First(cell => cell.IsOccupied);
        originalGame.SelectCellCommand.Execute(occupied);

        shell.NewGameCommand.Execute(null);

        Assert.Equal(ShellScreen.Playing, shell.CurrentScreen);
        Assert.Same(originalGame, shell.Game);
        Assert.Equal(0, shell.Game.Score);
        Assert.DoesNotContain(shell.Game.Cells, cell => cell.IsSelected);
    }

    private sealed class SequenceRandomSource : IRandomSource
    {
        public int Next(int exclusiveMax)
        {
            return 0;
        }
    }

    private sealed class RecordingSoundPlayer : ISoundPlayer
    {
        public List<SoundCue> Cues { get; } = new();

        public void Play(SoundCue cue)
        {
            Cues.Add(cue);
        }
    }
}

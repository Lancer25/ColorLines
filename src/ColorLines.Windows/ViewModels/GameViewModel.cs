using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;
using ColorLines.Core.Storage;
using ColorLines.Windows.Services;
using ColorLines.Windows.Themes;

namespace ColorLines.Windows.ViewModels;

public sealed class GameViewModel : INotifyPropertyChanged
{
    private readonly GameEngine engine;
    private GameState state;
    private BoardPosition? selectedPosition;
    private int score;
    private int highScore;
    private string statusText;
    private TurnFeedback feedback;
    private bool isSoundEnabled;
    private string animationIntensity;
    private HashSet<BoardPosition> movedPositions;
    private HashSet<BoardPosition> spawnedPositions;
    private HashSet<BoardPosition> clearedPositions;
    private HashSet<BoardPosition> rejectedPositions;
    private HashSet<BoardPosition> pathPreviewPositions;

    public GameViewModel(GameEngine engine, GameState state, int highScore = 0)
    {
        this.engine = engine;
        this.state = state;
        score = state.Score;
        this.highScore = Math.Max(highScore, score);
        statusText = "Select a cat to move.";
        feedback = TurnFeedback.Neutral;
        isSoundEnabled = true;
        animationIntensity = "Full";
        movedPositions = new HashSet<BoardPosition>();
        spawnedPositions = new HashSet<BoardPosition>();
        clearedPositions = new HashSet<BoardPosition>();
        rejectedPositions = new HashSet<BoardPosition>();
        pathPreviewPositions = new HashSet<BoardPosition>();
        Cells = new ObservableCollection<CellViewModel>();
        NextPieces = new ObservableCollection<PieceViewModel>();
        SelectCellCommand = new RelayCommand(SelectCell, parameter => parameter is CellViewModel);
        NewGameCommand = new RelayCommand(_ => NewGame());
        ToggleSoundCommand = new RelayCommand(_ => IsSoundEnabled = !IsSoundEnabled);
        PreviewPathCommand = new RelayCommand(PreviewPath, parameter => parameter is CellViewModel);
        ClearPreviewPathCommand = new RelayCommand(_ => ClearPreviewPath());
        RefreshFromState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CellViewModel> Cells { get; }

    public ObservableCollection<PieceViewModel> NextPieces { get; }

    public ICommand SelectCellCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand ToggleSoundCommand { get; }

    public ICommand PreviewPathCommand { get; }

    public ICommand ClearPreviewPathCommand { get; }

    public string SelectedThemeName => ThemeCatalog.DefaultTheme.DisplayName;

    public int Score
    {
        get => score;
        private set
        {
            if (score != value)
            {
                score = value;
                if (score > HighScore)
                {
                    HighScore = score;
                }
                OnPropertyChanged();
            }
        }
    }

    public int HighScore
    {
        get => highScore;
        private set
        {
            if (highScore != value)
            {
                highScore = value;
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

    public TurnFeedback Feedback
    {
        get => feedback;
        private set
        {
            if (feedback != value)
            {
                feedback = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameOver));
                OnPropertyChanged(nameof(ScoreDeltaText));
            }
        }
    }

    public bool IsGameOver => state.Status == GameStatus.GameOver || Feedback.IsGameOver;

    public string ScoreDeltaText => Feedback.HasScore ? $"+{Feedback.ScoreDelta}" : string.Empty;

    public bool IsSoundEnabled
    {
        get => isSoundEnabled;
        private set
        {
            if (isSoundEnabled != value)
            {
                isSoundEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public string AnimationIntensity
    {
        get => animationIntensity;
        private set
        {
            if (animationIntensity != value)
            {
                animationIntensity = value;
                OnPropertyChanged();
            }
        }
    }

    public static GameViewModel CreateForNewGame()
    {
        var engine = new GameEngine(new SystemRandomSource());
        return new GameViewModel(engine, engine.NewGame());
    }

    public static GameViewModel CreateFromSave(LocalSaveData? save)
    {
        var engine = new GameEngine(new SystemRandomSource());
        var state = save?.Game is null ? engine.NewGame() : GameSnapshotMapper.ToState(save.Game);
        var viewModel = new GameViewModel(engine, state, save?.HighScore ?? 0);
        if (save is not null)
        {
            viewModel.isSoundEnabled = save.IsSoundEnabled;
            viewModel.animationIntensity = save.AnimationIntensity;
        }

        return viewModel;
    }

    public LocalSaveData CreateSaveData(WindowPlacementData window)
    {
        return new LocalSaveData(
            1,
            HighScore,
            IsSoundEnabled,
            AnimationIntensity,
            ThemeCatalog.DefaultTheme.Id,
            GameSnapshotMapper.FromState(state),
            window);
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
            ClearCellFeedback();
            pathPreviewPositions.Clear();
            selectedPosition = position;
            StatusText = $"Selected {piece}. Choose an empty cell.";
            RefreshFromState();
            return;
        }

        if (selectedPosition is null)
        {
            ClearCellFeedback();
            pathPreviewPositions.Clear();
            rejectedPositions.Add(position);
            Feedback = new TurnFeedback(false, true, false, false, false, 0);
            StatusText = "Select a cat before choosing a target.";
            RefreshFromState();
            return;
        }

        var result = engine.Move(state, selectedPosition.Value, position);
        state = result.State;
        selectedPosition = null;
        pathPreviewPositions.Clear();
        Score = state.Score;
        Feedback = BuildFeedback(result);
        StoreCellFeedback(result);
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = BuildStatusText(result);
        RefreshFromState();
    }

    private void NewGame()
    {
        state = engine.NewGame();
        selectedPosition = null;
        pathPreviewPositions.Clear();
        ClearCellFeedback();
        Score = state.Score;
        Feedback = TurnFeedback.Neutral;
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = "Select a cat to move.";
        RefreshFromState();
    }

    private void RefreshFromState()
    {
        Cells.Clear();
        foreach (var cell in state.Board.Cells())
        {
            var isSelected = selectedPosition == cell.Position;
            var wasMovedTo = movedPositions.Contains(cell.Position);
            var wasSpawned = spawnedPositions.Contains(cell.Position);
            var wasCleared = clearedPositions.Contains(cell.Position);
            var wasRejectedTarget = rejectedPositions.Contains(cell.Position);
            var isReachableTarget = IsReachableTarget(cell.Position);
            var isPathPreview = pathPreviewPositions.Contains(cell.Position);
            Cells.Add(cell.Piece is null
                ? CellViewModel.Empty(
                    cell.Position.Row,
                    cell.Position.Column,
                    wasMovedTo,
                    wasSpawned,
                    wasCleared,
                    wasRejectedTarget,
                    isReachableTarget,
                    isPathPreview)
                : CellViewModel.Occupied(
                    cell.Position.Row,
                    cell.Position.Column,
                    cell.Piece.Value,
                    isSelected,
                    wasMovedTo,
                    wasSpawned,
                    wasCleared,
                    wasRejectedTarget,
                    isPathPreview));
        }

        NextPieces.Clear();
        foreach (var piece in state.NextPieces)
        {
            NextPieces.Add(PieceViewModel.FromPiece(piece));
        }
    }

    private void PreviewPath(object? parameter)
    {
        if (selectedPosition is null || parameter is not CellViewModel cell || cell.IsOccupied)
        {
            ClearPreviewPath();
            return;
        }

        var target = new BoardPosition(cell.Row, cell.Column);
        var path = PathFinder.FindPath(state.Board, selectedPosition.Value, target);
        if (path.Count == 0)
        {
            ClearPreviewPath();
            return;
        }

        var nextPreview = path.ToHashSet();
        if (pathPreviewPositions.SetEquals(nextPreview))
        {
            return;
        }

        pathPreviewPositions = nextPreview;
        RefreshFromState();
    }

    private void ClearPreviewPath()
    {
        if (pathPreviewPositions.Count == 0)
        {
            return;
        }

        pathPreviewPositions.Clear();
        RefreshFromState();
    }

    private bool IsReachableTarget(BoardPosition position)
    {
        return selectedPosition is not null
            && state.Board.GetPiece(position) is null
            && PathFinder.FindPath(state.Board, selectedPosition.Value, position).Count > 0;
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

    private void StoreCellFeedback(GameTurnResult result)
    {
        ClearCellFeedback();

        foreach (var gameEvent in result.Events)
        {
            switch (gameEvent.Kind)
            {
                case GameEventKind.PieceMoved:
                    if (gameEvent.Positions.Count > 0)
                    {
                        movedPositions.Add(gameEvent.Positions[^1]);
                    }

                    break;
                case GameEventKind.PiecesSpawned:
                    spawnedPositions.UnionWith(gameEvent.Positions);
                    break;
                case GameEventKind.LinesCleared:
                    clearedPositions.UnionWith(gameEvent.Positions);
                    break;
                case GameEventKind.MoveRejected:
                    if (gameEvent.Positions.Count > 0)
                    {
                        rejectedPositions.Add(gameEvent.Positions[^1]);
                    }

                    break;
            }
        }
    }

    private void ClearCellFeedback()
    {
        movedPositions.Clear();
        spawnedPositions.Clear();
        clearedPositions.Clear();
        rejectedPositions.Clear();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

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
    private readonly ISoundPlayer soundPlayer;
    private GameState state;
    private BoardPosition? selectedPosition;
    private int score;
    private int highScore;
    private string statusText;
    private TurnFeedback feedback;
    private bool isSoundEnabled;
    private string animationIntensity;
    private HashSet<BoardPosition> movedPositions;
    private HashSet<BoardPosition> movePathPositions;
    private HashSet<BoardPosition> spawnedPositions;
    private HashSet<BoardPosition> clearedPositions;
    private HashSet<BoardPosition> rejectedPositions;
    private HashSet<BoardPosition> pathPreviewPositions;
    private BoardPosition? pathPreviewTargetPosition;

    public GameViewModel(GameEngine engine, GameState state, int highScore = 0, ISoundPlayer? soundPlayer = null)
    {
        this.engine = engine;
        this.soundPlayer = soundPlayer ?? NullSoundPlayer.Instance;
        this.state = state;
        score = state.Score;
        this.highScore = Math.Max(highScore, score);
        statusText = "Select a cat to move.";
        feedback = TurnFeedback.Neutral;
        isSoundEnabled = true;
        animationIntensity = "Full";
        movedPositions = new HashSet<BoardPosition>();
        movePathPositions = new HashSet<BoardPosition>();
        spawnedPositions = new HashSet<BoardPosition>();
        clearedPositions = new HashSet<BoardPosition>();
        rejectedPositions = new HashSet<BoardPosition>();
        pathPreviewPositions = new HashSet<BoardPosition>();
        Cells = new ObservableCollection<CellViewModel>();
        NextPieces = new ObservableCollection<PieceViewModel>();
        SelectCellCommand = new RelayCommand(SelectCell, parameter => parameter is CellViewModel);
        NewGameCommand = new RelayCommand(_ => NewGame());
        EndGameCommand = new RelayCommand(_ => EndGame());
        ToggleSoundCommand = new RelayCommand(_ => IsSoundEnabled = !IsSoundEnabled);
        ToggleAnimationCommand = new RelayCommand(_ => ToggleAnimation());
        PreviewPathCommand = new RelayCommand(PreviewPath, parameter => parameter is CellViewModel);
        ClearPreviewPathCommand = new RelayCommand(_ => ClearPreviewPath());
        RefreshFromState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CellViewModel> Cells { get; }

    public ObservableCollection<PieceViewModel> NextPieces { get; }

    public ICommand SelectCellCommand { get; }

    public ICommand NewGameCommand { get; }

    public ICommand EndGameCommand { get; }

    public ICommand ToggleSoundCommand { get; }

    public ICommand ToggleAnimationCommand { get; }

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
                OnPropertyChanged(nameof(FinalScoreText));
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
                OnPropertyChanged(nameof(BestScoreText));
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
                OnPropertyChanged(nameof(ShowScoreDelta));
                OnPropertyChanged(nameof(ScoreDeltaBadgeText));
            }
        }
    }

    public bool IsGameOver => state.Status == GameStatus.GameOver || Feedback.IsGameOver;

    public string GameOverTitle => "Game Over";

    public string GameOverSummaryText => "The board is full. Start a new run?";

    public string FinalScoreText => $"Final Score: {Score}";

    public string BestScoreText => $"Best Score: {HighScore}";

    public string ScoreDeltaText => Feedback.HasScore ? $"+{Feedback.ScoreDelta}" : string.Empty;

    public bool ShowScoreDelta => Feedback.HasScore;

    public string ScoreDeltaBadgeText => ScoreDeltaText;

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
                OnPropertyChanged(nameof(IsFullAnimation));
                OnPropertyChanged(nameof(AnimationToggleText));
            }
        }
    }

    public bool IsFullAnimation => AnimationIntensity == "Full";

    public string AnimationToggleText => IsFullAnimation ? "Use Reduced Animation" : "Use Full Animation";

    public static GameViewModel CreateForNewGame()
    {
        var engine = new GameEngine(new SystemRandomSource());
        return new GameViewModel(engine, engine.NewGame());
    }

    public static GameViewModel CreateFromSave(LocalSaveData? save)
    {
        var engine = new GameEngine(new SystemRandomSource());
        var state = save?.Game is null ? engine.NewGame() : GameSnapshotMapper.ToState(save.Game);
        var viewModel = new GameViewModel(engine, state, save?.HighScore ?? 0, SystemSoundPlayer.Instance);
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
            pathPreviewTargetPosition = null;
            Feedback = TurnFeedback.Neutral;
            selectedPosition = position;
            StatusText = $"Selected {piece}. Choose an empty cell.";
            PlaySound(SoundCue.Select);
            RefreshFromState();
            return;
        }

        if (selectedPosition is null)
        {
            ClearCellFeedback();
            pathPreviewPositions.Clear();
            pathPreviewTargetPosition = null;
            rejectedPositions.Add(position);
            Feedback = new TurnFeedback(false, true, false, false, false, 0);
            StatusText = "Select a cat before choosing a target.";
            PlaySound(SoundCue.Reject);
            RefreshFromState();
            return;
        }

        var result = engine.Move(state, selectedPosition.Value, position);
        state = result.State;
        selectedPosition = null;
        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        Score = state.Score;
        Feedback = BuildFeedback(result);
        StoreCellFeedback(result);
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = BuildStatusText(result);
        PlayTurnSound(Feedback);
        RefreshFromState();
    }

    private void NewGame()
    {
        state = engine.NewGame();
        selectedPosition = null;
        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        ClearCellFeedback();
        Score = state.Score;
        Feedback = TurnFeedback.Neutral;
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = "Select a cat to move.";
        PlaySound(SoundCue.NewGame);
        RefreshFromState();
    }

    private void EndGame()
    {
        selectedPosition = null;
        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        ClearCellFeedback();
        Feedback = new TurnFeedback(false, false, false, false, true, 0);
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = "Game over. Start a new game?";
        PlaySound(SoundCue.GameOver);
        RefreshFromState();
    }

    private void ToggleAnimation()
    {
        AnimationIntensity = IsFullAnimation ? "Reduced" : "Full";
        RefreshFromState();
    }

    private void PlayTurnSound(TurnFeedback turnFeedback)
    {
        if (turnFeedback.IsGameOver)
        {
            PlaySound(SoundCue.GameOver);
            return;
        }

        if (turnFeedback.HadClear)
        {
            PlaySound(SoundCue.Clear);
            return;
        }

        if (turnFeedback.WasRejected)
        {
            PlaySound(SoundCue.Reject);
            return;
        }

        if (turnFeedback.WasMoved)
        {
            PlaySound(SoundCue.Move);
        }
    }

    private void PlaySound(SoundCue cue)
    {
        if (IsSoundEnabled)
        {
            soundPlayer.Play(cue);
        }
    }

    private void RefreshFromState()
    {
        EnsureCellsInitialized();
        var reachableTargets = GetReachableTargets();
        foreach (var cell in state.Board.Cells())
        {
            var isSelected = selectedPosition == cell.Position;
            var wasMovedTo = IsFullAnimation && movedPositions.Contains(cell.Position);
            var wasMovePath = IsFullAnimation && movePathPositions.Contains(cell.Position);
            var wasSpawned = IsFullAnimation && spawnedPositions.Contains(cell.Position);
            var wasCleared = IsFullAnimation && clearedPositions.Contains(cell.Position);
            var wasRejectedTarget = IsFullAnimation && rejectedPositions.Contains(cell.Position);
            var isReachableTarget = reachableTargets.Contains(cell.Position);
            var isPathPreview = pathPreviewPositions.Contains(cell.Position);
            var isPathPreviewTarget = pathPreviewTargetPosition == cell.Position;
            var cellViewModel = Cells[(cell.Position.Row * BoardPosition.BoardSize) + cell.Position.Column];
            cellViewModel.Update(
                cell.Piece,
                isSelected,
                wasMovedTo,
                wasMovePath,
                wasSpawned,
                wasCleared,
                wasRejectedTarget,
                isReachableTarget,
                isPathPreview,
                isPathPreviewTarget);
        }

        NextPieces.Clear();
        foreach (var piece in state.NextPieces)
        {
            NextPieces.Add(PieceViewModel.FromPiece(piece));
        }
    }

    private void EnsureCellsInitialized()
    {
        if (Cells.Count > 0)
        {
            return;
        }

        for (var row = 0; row < BoardPosition.BoardSize; row++)
        {
            for (var column = 0; column < BoardPosition.BoardSize; column++)
            {
                Cells.Add(CellViewModel.Empty(row, column));
            }
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
        if (pathPreviewPositions.SetEquals(nextPreview) && pathPreviewTargetPosition == target)
        {
            return;
        }

        pathPreviewPositions = nextPreview;
        pathPreviewTargetPosition = target;
        ApplyPathPreview();
    }

    private void ClearPreviewPath()
    {
        if (pathPreviewPositions.Count == 0)
        {
            return;
        }

        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        ApplyPathPreview();
    }

    private void ApplyPathPreview()
    {
        foreach (var cell in Cells)
        {
            var position = new BoardPosition(cell.Row, cell.Column);
            cell.SetPathPreview(
                pathPreviewPositions.Contains(position),
                pathPreviewTargetPosition == position);
        }
    }

    private HashSet<BoardPosition> GetReachableTargets()
    {
        if (selectedPosition is null)
        {
            return new HashSet<BoardPosition>();
        }

        var source = selectedPosition.Value;
        var reachable = new HashSet<BoardPosition>();
        var visited = new HashSet<BoardPosition> { source };
        var queue = new Queue<BoardPosition>();
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in current.OrthogonalNeighbors())
            {
                if (visited.Contains(next) || !state.Board.IsEmpty(next))
                {
                    continue;
                }

                visited.Add(next);
                reachable.Add(next);
                queue.Enqueue(next);
            }
        }

        return reachable;
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
                        movePathPositions.UnionWith(gameEvent.Positions);
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
        movePathPositions.Clear();
        spawnedPositions.Clear();
        clearedPositions.Clear();
        rejectedPositions.Clear();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

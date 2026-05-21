using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;
using ColorLines.Core.Storage;
using ColorLines.Windows.Localization;
using ColorLines.Windows.Services;
using ColorLines.Windows.Themes;

namespace ColorLines.Windows.ViewModels;

public sealed class GameViewModel : INotifyPropertyChanged
{
    private GameEngine engine;
    private readonly ISoundPlayer soundPlayer;
    private readonly UiTextProvider text;
    private GameState state;
    private BoardPosition? selectedPosition;
    private int score;
    private int highScore;
    private string statusText;
    private TurnFeedback feedback;
    private bool isSoundEnabled;
    private bool isPathHintsEnabled;
    private bool isAutoSaveEnabled;
    private string animationIntensity;
    private string difficulty;
    private HashSet<BoardPosition> movedPositions;
    private HashSet<BoardPosition> movePathPositions;
    private HashSet<BoardPosition> spawnedPositions;
    private HashSet<BoardPosition> clearedPositions;
    private HashSet<BoardPosition> rejectedPositions;
    private HashSet<BoardPosition> pathPreviewPositions;
    private BoardPosition? pathPreviewTargetPosition;

    public GameViewModel(GameEngine engine, GameState state, int highScore = 0, ISoundPlayer? soundPlayer = null, string? difficulty = null, UiTextProvider? text = null)
    {
        this.engine = engine;
        this.soundPlayer = soundPlayer ?? NullSoundPlayer.Instance;
        this.text = text ?? new UiTextProvider();
        this.state = state;
        score = state.Score;
        this.highScore = Math.Max(highScore, score);
        statusText = this.text.SelectCatToMove;
        feedback = TurnFeedback.Neutral;
        isSoundEnabled = true;
        isPathHintsEnabled = true;
        isAutoSaveEnabled = true;
        animationIntensity = "Full";
        this.difficulty = DifficultyCatalog.Normalize(difficulty);
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
        TogglePathHintsCommand = new RelayCommand(_ => TogglePathHints());
        ToggleAutoSaveCommand = new RelayCommand(_ => IsAutoSaveEnabled = !IsAutoSaveEnabled);
        ToggleAnimationCommand = new RelayCommand(_ => ToggleAnimation());
        SetDifficultyCommand = new RelayCommand(SetDifficulty);
        SetLanguageCommand = new RelayCommand(SetLanguage);
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

    public ICommand TogglePathHintsCommand { get; }

    public ICommand ToggleAutoSaveCommand { get; }

    public ICommand ToggleAnimationCommand { get; }

    public ICommand SetDifficultyCommand { get; }

    public ICommand SetLanguageCommand { get; }

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

    public string GameOverTitle => text.GameOverTitle;

    public string GameOverSummaryText => text.GameOverSummary;

    public string FinalScoreText => text.FinalScore(Score);

    public string BestScoreText => text.BestScore(HighScore);

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

    public bool IsPathHintsEnabled
    {
        get => isPathHintsEnabled;
        private set
        {
            if (isPathHintsEnabled != value)
            {
                isPathHintsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsAutoSaveEnabled
    {
        get => isAutoSaveEnabled;
        private set
        {
            if (isAutoSaveEnabled != value)
            {
                isAutoSaveEnabled = value;
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

    public string AnimationToggleText => IsFullAnimation ? text.UseReducedAnimation : text.UseFullAnimation;

    public string Difficulty
    {
        get => difficulty;
        private set
        {
            var next = DifficultyCatalog.Normalize(value);
            if (difficulty != next)
            {
                difficulty = next;
                OnPropertyChanged();
            }
        }
    }

    public int BoardSize => state.Board.Size;

    public int TotalCellCount => BoardSize * BoardSize;

    public int OccupiedCellCount => state.Board.Cells().Count(cell => cell.Piece is not null);

    public int EmptyCellCount => TotalCellCount - OccupiedCellCount;

    public int BoardFillPercent => TotalCellCount == 0
        ? 0
        : (int)Math.Round(OccupiedCellCount * 100.0 / TotalCellCount, MidpointRounding.AwayFromZero);

    public string BoardPressureLevel => BoardFillPercent switch
    {
        >= 75 => "Critical",
        >= 50 => "Tight",
        _ => "Calm"
    };

    public int IncomingPieceCount => Math.Min(state.NextPieces.Count, EmptyCellCount);

    public int ProjectedOccupiedCellCount => OccupiedCellCount + IncomingPieceCount;

    public int ProjectedBoardFillPercent => TotalCellCount == 0
        ? 0
        : (int)Math.Round(ProjectedOccupiedCellCount * 100.0 / TotalCellCount, MidpointRounding.AwayFromZero);

    public string ProjectedBoardPressureLevel => ProjectedBoardFillPercent switch
    {
        >= 75 => "Critical",
        >= 50 => "Tight",
        _ => "Calm"
    };

    public string Language => text.Language;

    public static GameViewModel CreateForNewGame()
    {
        var engine = new GameEngine(new SystemRandomSource());
        return new GameViewModel(engine, engine.NewGame());
    }

    public static GameViewModel CreateFromSave(LocalSaveData? save)
    {
        var difficulty = DifficultyCatalog.Normalize(save?.Difficulty);
        var languageText = new UiTextProvider(save?.Language);
        var engine = new GameEngine(new SystemRandomSource(), new GameOptions(BoardSize: DifficultyCatalog.ToBoardSize(difficulty)));
        var state = save?.Game is null ? engine.NewGame() : GameSnapshotMapper.ToState(save.Game);
        var viewModel = new GameViewModel(engine, state, save?.HighScore ?? 0, SystemSoundPlayer.Instance, difficulty, languageText);
        if (save is not null)
        {
            viewModel.isSoundEnabled = save.IsSoundEnabled;
            viewModel.animationIntensity = NormalizeAnimationIntensity(save.AnimationIntensity);
            viewModel.isPathHintsEnabled = save.IsPathHintsEnabled;
            viewModel.isAutoSaveEnabled = save.IsAutoSaveEnabled;
            viewModel.RefreshFromState();
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
            window)
        {
            Difficulty = Difficulty,
            Language = Language,
            IsPathHintsEnabled = IsPathHintsEnabled,
            IsAutoSaveEnabled = IsAutoSaveEnabled
        };
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
            StatusText = BuildSelectedStatusText(piece.Value);
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
            StatusText = text.SelectCatBeforeTarget;
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
        engine = new GameEngine(new SystemRandomSource(), new GameOptions(BoardSize: DifficultyCatalog.ToBoardSize(Difficulty)));
        state = engine.NewGame();
        selectedPosition = null;
        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        ClearCellFeedback();
        Score = state.Score;
        Feedback = TurnFeedback.Neutral;
        OnPropertyChanged(nameof(IsGameOver));
        OnPropertyChanged(nameof(BoardSize));
        StatusText = text.SelectCatToMove;
        PlaySound(SoundCue.NewGame);
        RefreshFromState();
    }

    private void EndGame()
    {
        state = state with { Status = GameStatus.GameOver };
        selectedPosition = null;
        pathPreviewPositions.Clear();
        pathPreviewTargetPosition = null;
        ClearCellFeedback();
        Feedback = new TurnFeedback(false, false, false, false, true, 0);
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = text.GameOverStatus;
        PlaySound(SoundCue.GameOver);
        RefreshFromState();
    }

    private void ToggleAnimation()
    {
        AnimationIntensity = IsFullAnimation ? "Reduced" : "Full";
        RefreshFromState();
    }

    private static string NormalizeAnimationIntensity(string? value)
    {
        return value is "Full" or "Reduced" ? value : "Full";
    }

    private void SetDifficulty(object? parameter)
    {
        if (parameter is string value)
        {
            Difficulty = value;
        }
    }

    private void SetLanguage(object? parameter)
    {
        if (parameter is not string value)
        {
            return;
        }

        text.SetLanguage(value);
        StatusText = text.SelectCatToMove;
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(GameOverTitle));
        OnPropertyChanged(nameof(GameOverSummaryText));
        OnPropertyChanged(nameof(FinalScoreText));
        OnPropertyChanged(nameof(BestScoreText));
        OnPropertyChanged(nameof(AnimationToggleText));
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

    private void TogglePathHints()
    {
        IsPathHintsEnabled = !IsPathHintsEnabled;
        if (!IsPathHintsEnabled)
        {
            pathPreviewPositions.Clear();
            pathPreviewTargetPosition = null;
        }

        RefreshFromState();
    }

    private void RefreshFromState()
    {
        EnsureCellsInitialized();
        var reachableTargets = GetReachableTargets();
        var clearOpportunities = GetClearOpportunities(reachableTargets);
        foreach (var cell in state.Board.Cells())
        {
            var isSelected = selectedPosition == cell.Position;
            var wasMovedTo = IsFullAnimation && movedPositions.Contains(cell.Position);
            var wasMovePath = IsFullAnimation && movePathPositions.Contains(cell.Position);
            var wasSpawned = IsFullAnimation && spawnedPositions.Contains(cell.Position);
            var wasCleared = IsFullAnimation && clearedPositions.Contains(cell.Position);
            var wasRejectedTarget = IsFullAnimation && rejectedPositions.Contains(cell.Position);
            var isReachableTarget = reachableTargets.Contains(cell.Position);
            var isClearOpportunity = clearOpportunities.Contains(cell.Position);
            var isPathPreview = pathPreviewPositions.Contains(cell.Position);
            var isPathPreviewTarget = pathPreviewTargetPosition == cell.Position;
            var cellViewModel = Cells[(cell.Position.Row * state.Board.Size) + cell.Position.Column];
            cellViewModel.Update(
                cell.Piece,
                isSelected,
                wasMovedTo,
                wasMovePath,
                wasSpawned,
                wasCleared,
                wasRejectedTarget,
                isReachableTarget,
                isClearOpportunity,
                isPathPreview,
                isPathPreviewTarget);
        }

        RefreshNextPieces();
        NotifyBoardMetricsChanged();
    }

    private void EnsureCellsInitialized()
    {
        var neededCount = state.Board.Size * state.Board.Size;
        if (Cells.Count == neededCount)
        {
            return;
        }

        Cells.Clear();
        for (var row = 0; row < state.Board.Size; row++)
        {
            for (var column = 0; column < state.Board.Size; column++)
            {
                Cells.Add(CellViewModel.Empty(row, column));
            }
        }
    }

    private void RefreshNextPieces()
    {
        if (NextPieces.Count == state.NextPieces.Count
            && NextPieces.Select(piece => piece.Kind).SequenceEqual(state.NextPieces))
        {
            return;
        }

        NextPieces.Clear();
        foreach (var piece in state.NextPieces)
        {
            NextPieces.Add(PieceViewModel.FromPiece(piece));
        }
    }

    private void PreviewPath(object? parameter)
    {
        if (!IsPathHintsEnabled || selectedPosition is null || parameter is not CellViewModel cell || cell.IsOccupied)
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
        if (pathPreviewPositions.Count == 0 && pathPreviewTargetPosition is null)
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
        if (!IsPathHintsEnabled || selectedPosition is null)
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
            foreach (var next in current.OrthogonalNeighbors(state.Board.Size))
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

    private HashSet<BoardPosition> GetClearOpportunities(IReadOnlySet<BoardPosition> reachableTargets)
    {
        if (!IsPathHintsEnabled || selectedPosition is null || reachableTargets.Count == 0)
        {
            return new HashSet<BoardPosition>();
        }

        var source = selectedPosition.Value;
        var selectedPiece = state.Board.GetPiece(source);
        if (selectedPiece is null)
        {
            return new HashSet<BoardPosition>();
        }

        var opportunities = new HashSet<BoardPosition>();
        foreach (var target in reachableTargets)
        {
            var previewBoard = state.Board.Clone();
            previewBoard.MovePiece(source, target);
            if (LineDetector.FindLines(previewBoard, new[] { target }).Count > 0)
            {
                opportunities.Add(target);
            }
        }

        return opportunities;
    }

    private string BuildSelectedStatusText(PieceKind piece)
    {
        var reachableTargets = GetReachableTargets();
        var clearOpportunityCount = GetClearOpportunities(reachableTargets).Count;
        return clearOpportunityCount == 0
            ? text.SelectedPiece(piece.ToString())
            : text.SelectedPieceWithClearOpportunities(piece.ToString(), clearOpportunityCount);
    }

    private string BuildStatusText(GameTurnResult result)
    {
        if (result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.MoveRejected))
        {
            return text.CannotMoveThere;
        }

        if (result.Events.Any(gameEvent => gameEvent.Kind == GameEventKind.GameOver))
        {
            return text.GameOverStatus;
        }

        var scoreEvent = result.Events.LastOrDefault(gameEvent => gameEvent.Kind == GameEventKind.ScoreChanged);
        if (scoreEvent is not null && scoreEvent.ScoreDelta > 0)
        {
            if (DidSpawnedPiecesCreateLine(result.Events))
            {
                return text.SpawnedLinePoints(scoreEvent.ScoreDelta);
            }

            return text.Points(scoreEvent.ScoreDelta);
        }

        return text.SelectCatToMove;
    }

    private static bool DidSpawnedPiecesCreateLine(IReadOnlyList<GameEvent> events)
    {
        var spawnIndex = -1;
        var clearIndex = -1;
        for (var index = 0; index < events.Count; index++)
        {
            if (events[index].Kind == GameEventKind.PiecesSpawned && spawnIndex < 0)
            {
                spawnIndex = index;
            }

            if (events[index].Kind == GameEventKind.LinesCleared && clearIndex < 0)
            {
                clearIndex = index;
            }
        }

        return spawnIndex >= 0 && clearIndex > spawnIndex;
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

    private void NotifyBoardMetricsChanged()
    {
        OnPropertyChanged(nameof(TotalCellCount));
        OnPropertyChanged(nameof(OccupiedCellCount));
        OnPropertyChanged(nameof(EmptyCellCount));
        OnPropertyChanged(nameof(BoardFillPercent));
        OnPropertyChanged(nameof(BoardPressureLevel));
        OnPropertyChanged(nameof(IncomingPieceCount));
        OnPropertyChanged(nameof(ProjectedOccupiedCellCount));
        OnPropertyChanged(nameof(ProjectedBoardFillPercent));
        OnPropertyChanged(nameof(ProjectedBoardPressureLevel));
    }
}

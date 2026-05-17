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
    private TurnFeedback feedback;

    public GameViewModel(GameEngine engine, GameState state)
    {
        this.engine = engine;
        this.state = state;
        score = state.Score;
        statusText = "Select a cat to move.";
        feedback = TurnFeedback.Neutral;
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
            Feedback = new TurnFeedback(false, true, false, false, false, 0);
            StatusText = "Select a cat before choosing a target.";
            return;
        }

        var result = engine.Move(state, selectedPosition.Value, position);
        state = result.State;
        selectedPosition = null;
        Score = state.Score;
        Feedback = BuildFeedback(result);
        OnPropertyChanged(nameof(IsGameOver));
        StatusText = BuildStatusText(result);
        RefreshFromState();
    }

    private void NewGame()
    {
        state = engine.NewGame();
        selectedPosition = null;
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

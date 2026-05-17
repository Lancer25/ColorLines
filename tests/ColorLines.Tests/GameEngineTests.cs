using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;

namespace ColorLines.Tests;

public sealed class GameEngineTests
{
    [Fact]
    public void NewGameSpawnsInitialPiecesAndNextPreview()
    {
        var engine = new GameEngine(new SequenceRandomSource(0, 1, 2, 3, 4, 5, 6, 7, 8, 9));

        var state = engine.NewGame();

        Assert.Equal(5, state.Board.Cells().Count(cell => cell.Piece is not null));
        Assert.Equal(3, state.NextPieces.Count);
        Assert.Equal(GameStatus.Playing, state.Status);
    }

    [Fact]
    public void MoveThatClearsLineDoesNotSpawnNewPieces()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 1), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 2), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 3), PieceKind.Orange);
        board.SetPiece(new BoardPosition(2, 4), PieceKind.Orange);

        var state = new GameState(board, new[] { PieceKind.Gray, PieceKind.Black, PieceKind.White }, 0, GameStatus.Playing);
        var engine = new GameEngine(new SequenceRandomSource(10, 11, 12));

        var result = engine.Move(state, new BoardPosition(2, 4), new BoardPosition(0, 4));

        Assert.Equal(10, result.State.Score);
        Assert.Equal(0, result.State.Board.Cells().Count(cell => cell.Piece is not null));
        Assert.Contains(result.Events, gameEvent => gameEvent.Kind == GameEventKind.LinesCleared);
        Assert.DoesNotContain(result.Events, gameEvent => gameEvent.Kind == GameEventKind.PiecesSpawned);
    }

    [Fact]
    public void MoveWithoutClearSpawnsNextPieces()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, new[] { PieceKind.Gray, PieceKind.Black, PieceKind.White }, 0, GameStatus.Playing);
        var engine = new GameEngine(new SequenceRandomSource(10, 11, 12, 0, 1, 2));

        var result = engine.Move(state, new BoardPosition(0, 0), new BoardPosition(0, 1));

        Assert.Equal(4, result.State.Board.Cells().Count(cell => cell.Piece is not null));
        Assert.Contains(result.Events, gameEvent => gameEvent.Kind == GameEventKind.PieceMoved);
        Assert.Contains(result.Events, gameEvent => gameEvent.Kind == GameEventKind.PiecesSpawned);
    }

    [Fact]
    public void InvalidMoveReturnsRejectedEvent()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 1), PieceKind.Gray);
        var state = new GameState(board, Array.Empty<PieceKind>(), 0, GameStatus.Playing);
        var engine = new GameEngine(new SequenceRandomSource());

        var result = engine.Move(state, new BoardPosition(0, 0), new BoardPosition(0, 1));

        Assert.Same(state, result.State);
        Assert.Single(result.Events);
        Assert.Equal(GameEventKind.MoveRejected, result.Events[0].Kind);
    }

    private sealed class SequenceRandomSource : IRandomSource
    {
        private readonly Queue<int> values;

        public SequenceRandomSource(params int[] values)
        {
            this.values = new Queue<int>(values);
        }

        public int Next(int exclusiveMax)
        {
            return values.Count == 0 ? 0 : values.Dequeue() % exclusiveMax;
        }
    }
}

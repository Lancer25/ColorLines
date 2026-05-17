using ColorLines.Core.Board;
using ColorLines.Core.Rules;

namespace ColorLines.Core.Game;

public sealed class GameEngine
{
    private static readonly PieceKind[] AllPieces = Enum.GetValues<PieceKind>();
    private readonly IRandomSource random;
    private readonly GameOptions options;

    public GameEngine(IRandomSource random, GameOptions? options = null)
    {
        this.random = random;
        this.options = options ?? GameOptions.Default;
    }

    public GameState NewGame()
    {
        var board = GameBoard.CreateEmpty();
        SpawnPieces(board, DrawPieces(options.InitialPieceCount), out _);
        return new GameState(board, DrawPieces(options.SpawnPieceCount), 0, GameStatus.Playing);
    }

    public GameTurnResult Move(GameState state, BoardPosition source, BoardPosition target)
    {
        if (state.Status == GameStatus.GameOver)
        {
            return new GameTurnResult(state, new[] { GameEvent.MoveRejected(source, target) });
        }

        var path = PathFinder.FindPath(state.Board, source, target);
        if (path.Count == 0)
        {
            return new GameTurnResult(state, new[] { GameEvent.MoveRejected(source, target) });
        }

        var board = state.Board.Clone();
        board.MovePiece(source, target);

        var events = new List<GameEvent> { GameEvent.PieceMoved(path) };
        var score = state.Score;

        var lines = LineDetector.FindLines(board, new[] { target });
        if (lines.Count > 0)
        {
            ApplyClear(board, lines, ref score, events);
            return new GameTurnResult(new GameState(board, state.NextPieces, score, GetStatus(board)), events);
        }

        var spawnedPositions = SpawnPieces(board, state.NextPieces, out var spawnedPieces);
        if (spawnedPositions.Count > 0)
        {
            events.Add(GameEvent.PiecesSpawned(spawnedPositions, spawnedPieces));

            var spawnLines = LineDetector.FindLines(board, spawnedPositions);
            if (spawnLines.Count > 0)
            {
                ApplyClear(board, spawnLines, ref score, events);
            }
        }

        var nextPieces = DrawPieces(options.SpawnPieceCount);
        var status = GetStatus(board);
        if (status == GameStatus.GameOver)
        {
            events.Add(GameEvent.GameOver());
        }

        return new GameTurnResult(new GameState(board, nextPieces, score, status), events);
    }

    private IReadOnlyList<PieceKind> DrawPieces(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => AllPieces[random.Next(AllPieces.Length)])
            .ToArray();
    }

    private IReadOnlyList<BoardPosition> SpawnPieces(GameBoard board, IReadOnlyList<PieceKind> pieces, out IReadOnlyList<PieceKind> spawnedPieces)
    {
        var positions = new List<BoardPosition>();
        var actualPieces = new List<PieceKind>();

        foreach (var piece in pieces)
        {
            var empty = board.EmptyPositions().ToArray();
            if (empty.Length == 0)
            {
                break;
            }

            var position = empty[random.Next(empty.Length)];
            board.SetPiece(position, piece);
            positions.Add(position);
            actualPieces.Add(piece);
        }

        spawnedPieces = actualPieces;
        return positions;
    }

    private static void ApplyClear(GameBoard board, IReadOnlyList<ClearedLine> lines, ref int score, List<GameEvent> events)
    {
        var clearPositions = LineDetector.UniquePositions(lines);
        foreach (var position in clearPositions)
        {
            board.ClearPiece(position);
        }

        var scoreDelta = ScoreCalculator.Calculate(lines.Count, clearPositions.Count);
        score += scoreDelta;
        events.Add(GameEvent.LinesCleared(lines, scoreDelta));
        events.Add(GameEvent.ScoreChanged(scoreDelta));
    }

    private static GameStatus GetStatus(GameBoard board)
    {
        return board.EmptyPositions().Any() ? GameStatus.Playing : GameStatus.GameOver;
    }
}

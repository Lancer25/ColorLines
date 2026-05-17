using ColorLines.Core.Board;
using ColorLines.Core.Rules;

namespace ColorLines.Core.Game;

public enum GameEventKind
{
    PieceSelected,
    MoveRejected,
    PieceMoved,
    LinesCleared,
    PiecesSpawned,
    ScoreChanged,
    GameOver
}

public sealed record GameEvent(
    GameEventKind Kind,
    IReadOnlyList<BoardPosition> Positions,
    IReadOnlyList<PieceKind> Pieces,
    IReadOnlyList<ClearedLine> Lines,
    int ScoreDelta)
{
    public static GameEvent MoveRejected(BoardPosition source, BoardPosition target)
    {
        return new GameEvent(GameEventKind.MoveRejected, new[] { source, target }, Array.Empty<PieceKind>(), Array.Empty<ClearedLine>(), 0);
    }

    public static GameEvent PieceMoved(IReadOnlyList<BoardPosition> path)
    {
        return new GameEvent(GameEventKind.PieceMoved, path, Array.Empty<PieceKind>(), Array.Empty<ClearedLine>(), 0);
    }

    public static GameEvent LinesCleared(IReadOnlyList<ClearedLine> lines, int scoreDelta)
    {
        return new GameEvent(GameEventKind.LinesCleared, LineDetector.UniquePositions(lines).ToArray(), Array.Empty<PieceKind>(), lines, scoreDelta);
    }

    public static GameEvent PiecesSpawned(IReadOnlyList<BoardPosition> positions, IReadOnlyList<PieceKind> pieces)
    {
        return new GameEvent(GameEventKind.PiecesSpawned, positions, pieces, Array.Empty<ClearedLine>(), 0);
    }

    public static GameEvent ScoreChanged(int scoreDelta)
    {
        return new GameEvent(GameEventKind.ScoreChanged, Array.Empty<BoardPosition>(), Array.Empty<PieceKind>(), Array.Empty<ClearedLine>(), scoreDelta);
    }

    public static GameEvent GameOver()
    {
        return new GameEvent(GameEventKind.GameOver, Array.Empty<BoardPosition>(), Array.Empty<PieceKind>(), Array.Empty<ClearedLine>(), 0);
    }
}

public sealed record GameTurnResult(GameState State, IReadOnlyList<GameEvent> Events);

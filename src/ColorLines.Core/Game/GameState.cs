using ColorLines.Core.Board;

namespace ColorLines.Core.Game;

public enum GameStatus
{
    Playing,
    GameOver
}

public sealed record GameState(
    GameBoard Board,
    IReadOnlyList<PieceKind> NextPieces,
    int Score,
    GameStatus Status);

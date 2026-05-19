using ColorLines.Core.Game;

namespace ColorLines.Core.Storage;

public sealed record PieceSnapshot(int Row, int Column, PieceKind Piece);

public sealed record GameSnapshot(
    int Version,
    IReadOnlyList<PieceSnapshot> Pieces,
    IReadOnlyList<PieceKind> NextPieces,
    int Score,
    GameStatus Status)
{
    public int BoardSize { get; init; } = 9;
}

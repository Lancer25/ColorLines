using ColorLines.Core.Game;

namespace ColorLines.Core.Board;

public sealed record Cell(BoardPosition Position, PieceKind? Piece);

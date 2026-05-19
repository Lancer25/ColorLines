using ColorLines.Core.Game;

namespace ColorLines.Core.Board;

public sealed class GameBoard
{
    private readonly PieceKind?[,] pieces;

    private GameBoard(PieceKind?[,] pieces)
    {
        this.pieces = pieces;
        Size = pieces.GetLength(0);
    }

    public int Size { get; }

    public static GameBoard CreateEmpty(int size = BoardPosition.DefaultBoardSize)
    {
        if (size < 5)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Board size must be at least 5.");
        }

        return new GameBoard(new PieceKind?[size, size]);
    }

    public static GameBoard FromPieces(IEnumerable<Cell> cells, int size = BoardPosition.DefaultBoardSize)
    {
        var board = CreateEmpty(size);
        foreach (var cell in cells)
        {
            if (cell.Piece is not null)
            {
                board.SetPiece(cell.Position, cell.Piece.Value);
            }
        }

        return board;
    }

    public PieceKind? GetPiece(BoardPosition position)
    {
        EnsureContains(position);
        return pieces[position.Row, position.Column];
    }

    public void SetPiece(BoardPosition position, PieceKind piece)
    {
        EnsureContains(position);
        pieces[position.Row, position.Column] = piece;
    }

    public void ClearPiece(BoardPosition position)
    {
        EnsureContains(position);
        pieces[position.Row, position.Column] = null;
    }

    public bool IsEmpty(BoardPosition position)
    {
        return GetPiece(position) is null;
    }

    public void MovePiece(BoardPosition source, BoardPosition target)
    {
        var piece = GetPiece(source);
        if (piece is null)
        {
            throw new InvalidOperationException("Source position does not contain a piece.");
        }

        if (!IsEmpty(target))
        {
            throw new InvalidOperationException("Target position is occupied.");
        }

        ClearPiece(source);
        SetPiece(target, piece.Value);
    }

    public IEnumerable<BoardPosition> AllPositions()
    {
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                yield return new BoardPosition(row, column);
            }
        }
    }

    public IEnumerable<BoardPosition> EmptyPositions()
    {
        return AllPositions().Where(IsEmpty);
    }

    public IEnumerable<Cell> Cells()
    {
        return AllPositions().Select(position => new Cell(position, GetPiece(position)));
    }

    public GameBoard Clone()
    {
        return new GameBoard((PieceKind?[,])pieces.Clone());
    }

    public bool Contains(BoardPosition position)
    {
        return position.Row >= 0
            && position.Row < Size
            && position.Column >= 0
            && position.Column < Size;
    }

    private void EnsureContains(BoardPosition position)
    {
        if (!Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, $"Position must be inside a {Size}x{Size} board.");
        }
    }
}

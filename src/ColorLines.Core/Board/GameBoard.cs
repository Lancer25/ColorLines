using ColorLines.Core.Game;

namespace ColorLines.Core.Board;

public sealed class GameBoard
{
    private readonly PieceKind?[,] pieces;

    private GameBoard(PieceKind?[,] pieces)
    {
        this.pieces = pieces;
    }

    public int Size => BoardPosition.BoardSize;

    public static GameBoard CreateEmpty()
    {
        return new GameBoard(new PieceKind?[BoardPosition.BoardSize, BoardPosition.BoardSize]);
    }

    public static GameBoard FromPieces(IEnumerable<Cell> cells)
    {
        var board = CreateEmpty();
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
        return pieces[position.Row, position.Column];
    }

    public void SetPiece(BoardPosition position, PieceKind piece)
    {
        pieces[position.Row, position.Column] = piece;
    }

    public void ClearPiece(BoardPosition position)
    {
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
}

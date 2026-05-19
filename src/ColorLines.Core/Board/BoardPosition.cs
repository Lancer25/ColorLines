namespace ColorLines.Core.Board;

public readonly record struct BoardPosition
{
    public const int DefaultBoardSize = 9;

    public BoardPosition(int row, int column)
    {
        if (row < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(row), row, "Row must be zero or greater.");
        }

        if (column < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be zero or greater.");
        }

        Row = row;
        Column = column;
    }

    public int Row { get; }

    public int Column { get; }

    public IEnumerable<BoardPosition> OrthogonalNeighbors(int boardSize)
    {
        if (Row > 0) yield return new BoardPosition(Row - 1, Column);
        if (Row < boardSize - 1) yield return new BoardPosition(Row + 1, Column);
        if (Column > 0) yield return new BoardPosition(Row, Column - 1);
        if (Column < boardSize - 1) yield return new BoardPosition(Row, Column + 1);
    }
}

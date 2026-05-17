namespace ColorLines.Core.Board;

public readonly record struct BoardPosition
{
    public const int BoardSize = 9;

    public BoardPosition(int row, int column)
    {
        if (row < 0 || row >= BoardSize)
        {
            throw new ArgumentOutOfRangeException(nameof(row), row, "Row must be between 0 and 8.");
        }

        if (column < 0 || column >= BoardSize)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be between 0 and 8.");
        }

        Row = row;
        Column = column;
    }

    public int Row { get; }

    public int Column { get; }

    public IEnumerable<BoardPosition> OrthogonalNeighbors()
    {
        if (Row > 0) yield return new BoardPosition(Row - 1, Column);
        if (Row < BoardSize - 1) yield return new BoardPosition(Row + 1, Column);
        if (Column > 0) yield return new BoardPosition(Row, Column - 1);
        if (Column < BoardSize - 1) yield return new BoardPosition(Row, Column + 1);
    }
}

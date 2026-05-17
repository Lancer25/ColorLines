using ColorLines.Core.Board;
using ColorLines.Core.Game;

namespace ColorLines.Core.Rules;

public sealed record ClearedLine(PieceKind Piece, IReadOnlyList<BoardPosition> Positions);

public static class LineDetector
{
    private static readonly (int Row, int Column)[] Directions =
    {
        (0, 1),
        (1, 0),
        (1, 1),
        (1, -1)
    };

    public static IReadOnlyList<ClearedLine> FindLines(GameBoard board, IEnumerable<BoardPosition> changedPositions)
    {
        var lines = new List<ClearedLine>();
        var seen = new HashSet<string>();

        foreach (var position in changedPositions.Distinct())
        {
            var piece = board.GetPiece(position);
            if (piece is null)
            {
                continue;
            }

            foreach (var direction in Directions)
            {
                var run = CollectRun(board, position, piece.Value, direction.Row, direction.Column);
                if (run.Count < 5)
                {
                    continue;
                }

                var key = string.Join("|", run.OrderBy(p => p.Row).ThenBy(p => p.Column).Select(p => $"{p.Row},{p.Column}"));
                if (seen.Add(key))
                {
                    lines.Add(new ClearedLine(piece.Value, run));
                }
            }
        }

        return lines;
    }

    public static IReadOnlySet<BoardPosition> UniquePositions(IEnumerable<ClearedLine> lines)
    {
        return lines.SelectMany(line => line.Positions).ToHashSet();
    }

    private static IReadOnlyList<BoardPosition> CollectRun(
        GameBoard board,
        BoardPosition origin,
        PieceKind piece,
        int rowDelta,
        int columnDelta)
    {
        var positions = new List<BoardPosition>();
        positions.AddRange(Walk(board, origin, piece, -rowDelta, -columnDelta).Reverse());
        positions.Add(origin);
        positions.AddRange(Walk(board, origin, piece, rowDelta, columnDelta));
        return positions;
    }

    private static IEnumerable<BoardPosition> Walk(
        GameBoard board,
        BoardPosition origin,
        PieceKind piece,
        int rowDelta,
        int columnDelta)
    {
        var row = origin.Row + rowDelta;
        var column = origin.Column + columnDelta;

        while (row >= 0 && row < BoardPosition.BoardSize && column >= 0 && column < BoardPosition.BoardSize)
        {
            var position = new BoardPosition(row, column);
            if (board.GetPiece(position) != piece)
            {
                yield break;
            }

            yield return position;
            row += rowDelta;
            column += columnDelta;
        }
    }
}

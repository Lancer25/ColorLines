using ColorLines.Core.Board;

namespace ColorLines.Core.Rules;

public static class PathFinder
{
    public static IReadOnlyList<BoardPosition> FindPath(GameBoard board, BoardPosition source, BoardPosition target)
    {
        if (!board.Contains(source) || !board.Contains(target) || source == target || board.GetPiece(source) is null || !board.IsEmpty(target))
        {
            return Array.Empty<BoardPosition>();
        }

        var queue = new Queue<BoardPosition>();
        var visited = new HashSet<BoardPosition> { source };
        var previous = new Dictionary<BoardPosition, BoardPosition>();

        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in current.OrthogonalNeighbors(board.Size))
            {
                if (visited.Contains(next))
                {
                    continue;
                }

                if (next != target && !board.IsEmpty(next))
                {
                    continue;
                }

                visited.Add(next);
                previous[next] = current;

                if (next == target)
                {
                    return ReconstructPath(previous, source, target);
                }

                queue.Enqueue(next);
            }
        }

        return Array.Empty<BoardPosition>();
    }

    public static bool CanReach(GameBoard board, BoardPosition source, BoardPosition target)
    {
        return FindPath(board, source, target).Count > 0;
    }

    private static IReadOnlyList<BoardPosition> ReconstructPath(
        IReadOnlyDictionary<BoardPosition, BoardPosition> previous,
        BoardPosition source,
        BoardPosition target)
    {
        var path = new List<BoardPosition> { target };
        var current = target;

        while (current != source)
        {
            current = previous[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}

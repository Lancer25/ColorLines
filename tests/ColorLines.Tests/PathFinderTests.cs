using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;

namespace ColorLines.Tests;

public sealed class PathFinderTests
{
    [Fact]
    public void FindsPathAcrossEmptyBoard()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);

        var path = PathFinder.FindPath(board, new BoardPosition(0, 0), new BoardPosition(0, 3));

        Assert.Equal(
            new[] { new BoardPosition(0, 0), new BoardPosition(0, 1), new BoardPosition(0, 2), new BoardPosition(0, 3) },
            path);
    }

    [Fact]
    public void ReturnsEmptyPathWhenTargetIsBlocked()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 1), PieceKind.Gray);
        board.SetPiece(new BoardPosition(1, 0), PieceKind.Gray);

        var path = PathFinder.FindPath(board, new BoardPosition(0, 0), new BoardPosition(2, 2));

        Assert.Empty(path);
    }

    [Fact]
    public void AllowsTargetOnlyWhenItIsEmpty()
    {
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        board.SetPiece(new BoardPosition(0, 1), PieceKind.Gray);

        var path = PathFinder.FindPath(board, new BoardPosition(0, 0), new BoardPosition(0, 1));

        Assert.Empty(path);
    }
}

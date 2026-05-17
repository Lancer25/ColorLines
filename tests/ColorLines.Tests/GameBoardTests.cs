using ColorLines.Core.Board;
using ColorLines.Core.Game;

namespace ColorLines.Tests;

public sealed class GameBoardTests
{
    [Fact]
    public void NewBoardCreatesEmptyNineByNineGrid()
    {
        var board = GameBoard.CreateEmpty();

        Assert.Equal(9, board.Size);
        Assert.Equal(81, board.EmptyPositions().Count());
        Assert.All(board.AllPositions(), position => Assert.Null(board.GetPiece(position)));
    }

    [Fact]
    public void SetPieceStoresPieceAtPosition()
    {
        var board = GameBoard.CreateEmpty();
        var position = new BoardPosition(2, 3);

        board.SetPiece(position, PieceKind.Orange);

        Assert.Equal(PieceKind.Orange, board.GetPiece(position));
        Assert.False(board.IsEmpty(position));
    }

    [Fact]
    public void MovePieceRequiresSourcePieceAndEmptyTarget()
    {
        var board = GameBoard.CreateEmpty();
        var source = new BoardPosition(0, 0);
        var target = new BoardPosition(0, 1);
        var occupied = new BoardPosition(0, 2);
        board.SetPiece(source, PieceKind.Gray);
        board.SetPiece(occupied, PieceKind.Black);

        Assert.Throws<InvalidOperationException>(() => board.MovePiece(target, source));
        Assert.Throws<InvalidOperationException>(() => board.MovePiece(source, occupied));

        board.MovePiece(source, target);

        Assert.Null(board.GetPiece(source));
        Assert.Equal(PieceKind.Gray, board.GetPiece(target));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    public void PositionRejectsCoordinatesOutsideBoard(int row, int column)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoardPosition(row, column));
    }
}

using ColorLines.Core.Board;
using ColorLines.Core.Game;

namespace ColorLines.Tests;

public sealed class GameBoardTests
{
    [Fact]
    public void NewBoardCreatesEmptyDefaultNineByNineGrid()
    {
        var board = GameBoard.CreateEmpty();

        Assert.Equal(9, board.Size);
        Assert.Equal(81, board.EmptyPositions().Count());
        Assert.All(board.AllPositions(), position => Assert.Null(board.GetPiece(position)));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(11)]
    public void NewBoardCanUseConfiguredSize(int size)
    {
        var board = GameBoard.CreateEmpty(size);

        Assert.Equal(size, board.Size);
        Assert.Equal(size * size, board.EmptyPositions().Count());
        Assert.Contains(new BoardPosition(size - 1, size - 1), board.AllPositions());
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
    public void PositionRejectsNegativeCoordinates(int row, int column)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoardPosition(row, column));
    }

    [Theory]
    [InlineData(7, 7, 0)]
    [InlineData(7, 0, 7)]
    public void BoardRejectsCoordinatesOutsideItsSize(int size, int row, int column)
    {
        var board = GameBoard.CreateEmpty(size);

        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetPiece(new BoardPosition(row, column)));
    }
}

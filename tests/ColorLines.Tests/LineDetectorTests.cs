using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;

namespace ColorLines.Tests;

public sealed class LineDetectorTests
{
    [Fact]
    public void DetectsHorizontalRunOfFive()
    {
        var board = GameBoard.CreateEmpty();
        for (var column = 1; column <= 5; column++)
        {
            board.SetPiece(new BoardPosition(3, column), PieceKind.Orange);
        }

        var lines = LineDetector.FindLines(board, new[] { new BoardPosition(3, 3) });

        Assert.Single(lines);
        Assert.Equal(5, lines[0].Positions.Count);
    }

    [Fact]
    public void DetectsVerticalDiagonalAndLongRuns()
    {
        var board = GameBoard.CreateEmpty();
        for (var offset = 0; offset < 6; offset++)
        {
            board.SetPiece(new BoardPosition(offset, offset), PieceKind.Calico);
        }

        var lines = LineDetector.FindLines(board, new[] { new BoardPosition(4, 4) });

        Assert.Single(lines);
        Assert.Equal(6, lines[0].Positions.Count);
        Assert.Equal(PieceKind.Calico, lines[0].Piece);
    }

    [Fact]
    public void MergesOverlappingPositionsForCrossClear()
    {
        var board = GameBoard.CreateEmpty();
        for (var column = 0; column < 5; column++)
        {
            board.SetPiece(new BoardPosition(4, column), PieceKind.Black);
        }

        for (var row = 2; row < 7; row++)
        {
            board.SetPiece(new BoardPosition(row, 2), PieceKind.Black);
        }

        var lines = LineDetector.FindLines(board, new[] { new BoardPosition(4, 2) });
        var clearPositions = LineDetector.UniquePositions(lines);

        Assert.Equal(2, lines.Count);
        Assert.Equal(9, clearPositions.Count);
    }

    [Fact]
    public void UsesTheBoardsConfiguredBounds()
    {
        var board = GameBoard.CreateEmpty(7);
        for (var column = 2; column < 7; column++)
        {
            board.SetPiece(new BoardPosition(6, column), PieceKind.White);
        }

        var lines = LineDetector.FindLines(board, new[] { new BoardPosition(6, 5) });

        Assert.Single(lines);
        Assert.Equal(5, lines[0].Positions.Count);
    }

    [Theory]
    [InlineData(1, 5, 10)]
    [InlineData(1, 6, 14)]
    [InlineData(2, 9, 36)]
    public void CalculatesScoreFromLineCountAndClearedPieces(int lineCount, int clearedPieces, int expectedScore)
    {
        Assert.Equal(expectedScore, ScoreCalculator.Calculate(lineCount, clearedPieces));
    }
}

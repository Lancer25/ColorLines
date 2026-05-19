using ColorLines.Core.Board;
using ColorLines.Core.Game;

namespace ColorLines.Core.Storage;

public static class GameSnapshotMapper
{
    public static GameSnapshot FromState(GameState state)
    {
        var pieces = state.Board.Cells()
            .Where(cell => cell.Piece is not null)
            .Select(cell => new PieceSnapshot(cell.Position.Row, cell.Position.Column, cell.Piece!.Value))
            .ToArray();

        return new GameSnapshot(1, pieces, state.NextPieces.ToArray(), state.Score, state.Status)
        {
            BoardSize = state.Board.Size
        };
    }

    public static GameState ToState(GameSnapshot snapshot)
    {
        if (snapshot.Version != 1)
        {
            throw new NotSupportedException($"Unsupported game snapshot version {snapshot.Version}.");
        }

        var boardSize = snapshot.BoardSize <= 0 ? BoardPosition.DefaultBoardSize : snapshot.BoardSize;
        var cells = snapshot.Pieces.Select(piece => new Cell(new BoardPosition(piece.Row, piece.Column), piece.Piece));
        var board = GameBoard.FromPieces(cells, boardSize);
        return new GameState(board, snapshot.NextPieces.ToArray(), snapshot.Score, snapshot.Status);
    }
}

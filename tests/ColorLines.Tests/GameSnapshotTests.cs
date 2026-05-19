using System.Text.Json;
using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Storage;

namespace ColorLines.Tests;

public sealed class GameSnapshotTests
{
    [Fact]
    public void ConvertsGameStateToSnapshotAndBack()
    {
        var board = GameBoard.CreateEmpty(11);
        board.SetPiece(new BoardPosition(1, 2), PieceKind.Tuxedo);
        board.SetPiece(new BoardPosition(10, 10), PieceKind.White);
        var state = new GameState(board, new[] { PieceKind.Gray, PieceKind.Black, PieceKind.Calico }, 42, GameStatus.Playing);

        var snapshot = GameSnapshotMapper.FromState(state);
        var restored = GameSnapshotMapper.ToState(snapshot);

        Assert.Equal(1, snapshot.Version);
        Assert.Equal(11, snapshot.BoardSize);
        Assert.Equal(42, restored.Score);
        Assert.Equal(GameStatus.Playing, restored.Status);
        Assert.Equal(11, restored.Board.Size);
        Assert.Equal(PieceKind.Tuxedo, restored.Board.GetPiece(new BoardPosition(1, 2)));
        Assert.Equal(PieceKind.White, restored.Board.GetPiece(new BoardPosition(10, 10)));
        Assert.Equal(state.NextPieces, restored.NextPieces);
    }

    [Fact]
    public void SnapshotSerializesWithSystemTextJson()
    {
        var state = new GameState(GameBoard.CreateEmpty(), new[] { PieceKind.Orange }, 7, GameStatus.GameOver);

        var json = JsonSerializer.Serialize(GameSnapshotMapper.FromState(state));
        var snapshot = JsonSerializer.Deserialize<GameSnapshot>(json);

        Assert.NotNull(snapshot);
        Assert.Equal(1, snapshot.Version);
        Assert.Equal(9, snapshot.BoardSize);
        Assert.Equal(7, snapshot.Score);
        Assert.Equal(GameStatus.GameOver, snapshot.Status);
    }

    [Fact]
    public void OldSnapshotsWithoutBoardSizeRestoreAsDefaultNineByNine()
    {
        var json = """
            {
              "Version": 1,
              "Pieces": [],
              "NextPieces": [ 0 ],
              "Score": 3,
              "Status": 0
            }
            """;

        var snapshot = JsonSerializer.Deserialize<GameSnapshot>(json);
        var restored = GameSnapshotMapper.ToState(snapshot!);

        Assert.Equal(9, restored.Board.Size);
    }
}

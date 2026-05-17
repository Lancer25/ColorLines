using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Storage;
using ColorLines.Windows.Services;

namespace ColorLines.Tests;

public sealed class LocalSaveServiceTests
{
    [Fact]
    public void SavesAndLoadsLocalDataAsJson()
    {
        var path = Path.Combine(Path.GetTempPath(), $"color-lines-{Guid.NewGuid():N}.json");
        var service = new LocalSaveService(path);
        var board = GameBoard.CreateEmpty();
        board.SetPiece(new BoardPosition(0, 0), PieceKind.Orange);
        var state = new GameState(board, new[] { PieceKind.Gray }, 12, GameStatus.Playing);
        var data = new LocalSaveData(
            Version: 1,
            HighScore: 40,
            IsSoundEnabled: false,
            AnimationIntensity: "Reduced",
            ThemeId: "CozyBoard",
            Game: GameSnapshotMapper.FromState(state),
            Window: new WindowPlacementData(900, 700));

        try
        {
            service.Save(data);
            var loaded = service.Load();

            Assert.NotNull(loaded);
            Assert.Equal(40, loaded.HighScore);
            Assert.False(loaded.IsSoundEnabled);
            Assert.Equal("Reduced", loaded.AnimationIntensity);
            Assert.Equal(900, loaded.Window.Width);
            Assert.Equal(PieceKind.Orange, GameSnapshotMapper.ToState(loaded.Game!).Board.GetPiece(new BoardPosition(0, 0)));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}

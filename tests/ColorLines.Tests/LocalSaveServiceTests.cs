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
            Window: new WindowPlacementData(900, 700))
        {
            Difficulty = "Hard",
            Language = "zh",
            IsPathHintsEnabled = false,
            IsAutoSaveEnabled = false
        };

        try
        {
            service.Save(data);
            var loaded = service.Load();

            Assert.NotNull(loaded);
            Assert.Equal(40, loaded.HighScore);
            Assert.False(loaded.IsSoundEnabled);
            Assert.Equal("Reduced", loaded.AnimationIntensity);
            Assert.Equal("Hard", loaded.Difficulty);
            Assert.Equal("zh", loaded.Language);
            Assert.False(loaded.IsPathHintsEnabled);
            Assert.False(loaded.IsAutoSaveEnabled);
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

    [Fact]
    public void SaveReplacesExistingFileAndCleansStaleTemporaryFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"color-lines-replace-{Guid.NewGuid():N}.json");
        var tempPath = $"{path}.tmp";
        var service = new LocalSaveService(path);
        var first = new LocalSaveData(1, 10, true, "Full", "CozyBoard", null, new WindowPlacementData(900, 700));
        var second = new LocalSaveData(1, 88, false, "Reduced", "CozyBoard", null, new WindowPlacementData(1000, 720));

        try
        {
            service.Save(first);
            File.WriteAllText(tempPath, "stale temp");

            service.Save(second);
            var loaded = service.Load();

            Assert.NotNull(loaded);
            Assert.Equal(88, loaded.HighScore);
            Assert.False(loaded.IsSoundEnabled);
            Assert.False(File.Exists(tempPath));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

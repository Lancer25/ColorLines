namespace ColorLines.Core.Game;

public sealed record GameOptions(int InitialPieceCount = 5, int SpawnPieceCount = 3, int BoardSize = 9)
{
    public static GameOptions Default { get; } = new();
}
